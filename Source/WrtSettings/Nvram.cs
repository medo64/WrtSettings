using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace WrtSettings {
    internal class Nvram {

        public Nvram(string fileName, NvramFormat format) {
            try {
                byte[] buffer;
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    buffer = new byte[(int)stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                }

                this.Variables = new Dictionary<string, string>();

                if (((format & NvramFormat.AsuswrtVersion1) != 0) && TryParseAsuswrtVersion1(buffer)) {
                } else if (((format & NvramFormat.AsuswrtVersion2) != 0) && TryParseAsuswrtVersion2(buffer)) {
                } else if (((format & NvramFormat.DDWrt) != 0) && TryParseDDWrt(buffer)) {
                } else if (((format & NvramFormat.Tomato) != 0) && TryParseTomato(buffer)) {
                } else if (((format & NvramFormat.Text) != 0) && TryParseText(buffer)) {
                } else {
                    throw new FormatException("Unrecognized format.");
                }
                this.FileName = fileName;
            } catch (FormatException) {
                throw;
            } catch (Exception ex) {
                throw new FormatException(ex.Message);
            }
        }


        public string FileName { get; private set; }
        public NvramFormat Format { get; set; }
        public IDictionary<String, String> Variables { get; private set; }


        public void Save(string fileName) {
            try {
                byte[] buffer;

                switch (this.Format) {
                    case NvramFormat.AsuswrtVersion1: buffer = GetBytesForAsuswrtVersion1(); break;
                    case NvramFormat.AsuswrtVersion2: buffer = GetBytesForAsuswrtVersion2(); break;
                    case NvramFormat.Tomato: buffer = GetBytesForTomato(); break;
                    case NvramFormat.DDWrt: buffer = GetBytesForDDWrt(); break;
                    case NvramFormat.Text: buffer = GetBytesForText(); break;
                    default: throw new InvalidOperationException("Unsupported format!");
                }

                using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    stream.Write(buffer, 0, buffer.Length);
                }
                this.FileName = fileName;
            } catch (InvalidOperationException) {
                throw;
            } catch (Exception ex) {
                throw new InvalidOperationException(ex.Message);
            }
        }


        #region Open

        private bool TryParseAsuswrtVersion1(byte[] buffer) {
            if (buffer.Length < 8) {
                Debug.WriteLine("NVRAM: Asuswrt 1: File is too small!");
                return false;
            }

            string header = ToString(buffer, 0, 4);
            if (!header.Equals("HDR1")) {
                Debug.WriteLine("NVRAM: Asuswrt 1: Unknown header '" + header + "'!");
                return false;
            }

            var len = ToUInt32(buffer, 4);
            if (len > (buffer.Length - 8)) {
                Debug.WriteLine("NVRAM: Asuswrt 1: length mismatch!");
                return false;
            }

            //decode variables
            var lastStart = 8;
            for (int i = 8; i < len + 8; i++) {
                if (buffer[i] == 0x00) {
                    if (i != lastStart) { //skip empty ones
                        var variable = Encoding.ASCII.GetString(buffer, lastStart, i - lastStart);
                        Debug.WriteLine("NVRAM: Asuswrt 1: " + variable);
                        var parts = variable.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2) {
                            this.Variables.Add(parts[0], parts[1]);
                        } else {
                            Debug.WriteLine("NVRAM: Asuswrt 1: Cannot parse '" + variable + "'!");
                            return false;
                        }
                    }
                    lastStart = i + 1;
                }
            }

            this.Format = NvramFormat.AsuswrtVersion1;
            return true;
        }

        private bool TryParseAsuswrtVersion2(byte[] buffer) {
            if (buffer.Length < 8) {
                Debug.WriteLine("NVRAM: Asuswrt 2: File is too small!");
                return false;
            }

            string header = ToString(buffer, 0, 4);
            if (!header.Equals("HDR2")) {
                Debug.WriteLine("NVRAM: Asuswrt 2: Unknown header '" + header + "'!");
                return false;
            }

            var len = ToUInt24(buffer, 4);
            if (len > (buffer.Length - 8)) {
                Debug.WriteLine("NVRAM: Asuswrt 2: Length mismatch!");
                return false;
            }

            //decoding stupid "encryption"
            var random = buffer[7];
            for (int i = 8; i < len + 8; i++) {
                if (buffer[i] > (0xFD - 0x01)) {
                    buffer[i] = 0x00;
                } else {
                    buffer[i] = (byte)((0xFF + random - buffer[i]) % 256);
                }
            }

            //decode variables
            var lastStart = 8;
            for (int i = 8; i < len + 8; i++) {
                if (buffer[i] == 0x00) {
                    if (i != lastStart) { //skip empty ones
                        var variable = Encoding.ASCII.GetString(buffer, lastStart, i - lastStart);
                        Debug.WriteLine("NVRAM: Asuswrt 2: " + variable);
                        var parts = variable.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2) {
                            this.Variables.Add(parts[0], parts[1]);
                        } else {
                            Debug.WriteLine("NVRAM: Asuswrt 2: Cannot parse '" + variable + "'!");
                            return false;
                        }
                    }
                    lastStart = i + 1;
                }
            }

            this.Format = NvramFormat.AsuswrtVersion2;
            return true;
        }

        private bool TryParseTomato(byte[] buffer) {
            try {
                using (var msIn = new MemoryStream(buffer) { Position = 0 })
                using (var msOut = new MemoryStream())
                using (var gzStream = new GZipStream(msIn, CompressionMode.Decompress)) {
                    var midBuffer = new byte[65536];
                    int read;
                    while ((read = gzStream.Read(midBuffer, 0, midBuffer.Length)) > 0) {
                        msOut.Write(midBuffer, 0, read);
                    }
                    buffer = msOut.ToArray();
                }
            } catch (InvalidDataException ex) {
                Debug.WriteLine("NVRAM: Tomato: Cannot ungzip (" + ex.Message + ")!");
                return false;
            }

            string header = ToString(buffer, 0, 4);
            if (!header.Equals("TCF1")) {
                Debug.WriteLine("NVRAM: Tomato: Unknown header '" + header + "'!");
                return false;
            }

            //decode variables
            var lastStart = 8;
            for (int i = 8; i < buffer.Length; i++) {
                if (buffer[i] == 0x00) {
                    if (i != lastStart) { //skip empty ones
                        var variable = Encoding.ASCII.GetString(buffer, lastStart, i - lastStart);
                        Debug.WriteLine("NVRAM: Tomato: " + variable);
                        var parts = variable.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2) {
                            this.Variables.Add(parts[0], parts[1]);
                        } else {
                            Debug.WriteLine("NVRAM: Tomato: Cannot parse '" + variable + "'!");
                            return false;
                        }
                    }
                    lastStart = i + 1;
                }
            }

            var hardwareType = ToUInt32(buffer, 4);
            this.Variables.Add(".HardwareType", hardwareType.ToString(CultureInfo.InvariantCulture));

            this.Format = NvramFormat.Tomato;
            return true;
        }

        private bool TryParseDDWrt(byte[] buffer) {
            if (buffer.Length < 8) {
                Debug.WriteLine("NVRAM: DDWrt: File is too small!");
                return false;
            }

            string header = ToString(buffer, 0, 6);
            if (!header.Equals("DD-WRT")) {
                Debug.WriteLine("NVRAM: DDWrt: Unknown header '" + header + "'!");
                return false;
            }

            var count = ToUInt16(buffer, 6);

            int count2 = 0;
            int i = 8;
            while (i < buffer.Length) {
                var keyLen = buffer[i];
                i += 1;

                var key = Encoding.ASCII.GetString(buffer, i, keyLen);
                i += keyLen;

                var valueLen = ToUInt16(buffer, i);
                i += 2;

                var value = Encoding.ASCII.GetString(buffer, i, valueLen);
                i += valueLen;

                Debug.WriteLine("NVRAM: DDWrt: " + key + "=" + value);
                count2 += 1;
                this.Variables.Add(key, value);
            }

            if (count != count2) {
                Debug.WriteLine("NVRAM: DDWrt: Length mismatch!");
                return false;
            }

            this.Format = NvramFormat.DDWrt;
            return true;
        }

        private bool TryParseText(byte[] buffer) {
            var content = Encoding.ASCII.GetString(buffer);
            var lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var variable in lines) {
                Debug.WriteLine("NVRAM: Text: " + variable);
                var parts = variable.Split(new char[] { '=' }, 2);
                if (parts.Length == 2) {
                    this.Variables.Add(DecodeText(parts[0]), DecodeText(parts[1]));
                } else {
                    Debug.WriteLine("NVRAM: Text: Cannot parse '" + variable + "'!");
                    return false;
                }
            }

            this.Format = NvramFormat.Text;
            return true;
        }

        #endregion


        #region Save

        private byte[] GetBytesForAsuswrtVersion1() {
            var buffer = new List<Byte>();

            foreach (var pair in this.Variables) {
                buffer.AddRange(Encoding.ASCII.GetBytes(pair.Key + "=" + pair.Value + "\0"));
            }

            //length has to be multiple of 1024
            var len = buffer.Count + (1024 - buffer.Count % 1024);

            //padding
            var padding = new byte[len - buffer.Count];
            buffer.AddRange(padding);

            //insert header in reverse order
            buffer.InsertRange(0, FromUInt32((uint)len));
            buffer.InsertRange(0, Encoding.ASCII.GetBytes("HDR1"));

            return buffer.ToArray();
        }

        private Random Random = new Random();

        private byte[] GetBytesForAsuswrtVersion2() {
            var buffer = new List<Byte>();

            var rand = (byte)Random.Next(0, 256);

            foreach (var pair in this.Variables) {
                buffer.AddRange(Encoding.ASCII.GetBytes(pair.Key + "=" + pair.Value + "\0"));
            }

            //length has to be multiple of 1024
            var len = buffer.Count + (1024 - buffer.Count % 1024);

            //padding
            var padding = new byte[len - buffer.Count];
            buffer.AddRange(padding);

            //do stupid "encryption"
            for (int i = 0; i < buffer.Count; i++) {
                if (buffer[i] == 0x00) {
                    buffer[i] = (byte)(0xFD + Random.Next(0, 3));
                } else {
                    buffer[i] = (byte)((0xFF - buffer[i] + rand) % 256);
                }
            }

            //insert header in reverse order
            buffer.Insert(0, rand);
            buffer.InsertRange(0, FromUInt24((uint)len));
            buffer.InsertRange(0, Encoding.ASCII.GetBytes("HDR2"));

            return buffer.ToArray();
        }

        private byte[] GetBytesForTomato() {
            var buffer = new List<Byte>();

            if (!this.Variables.ContainsKey(".HardwareType")) {
                throw new InvalidOperationException("Data format requires hardware type to be defined (.HardwareType)!");
            }

            var hardwareTypeText = this.Variables[".HardwareType"];
            uint hardwareType;
            if (!uint.TryParse(hardwareTypeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out hardwareType)) {
                throw new InvalidOperationException("Data format requires hardware type to be defined (.HardwareType) as an integer!");
            }

            buffer.AddRange(Encoding.ASCII.GetBytes("TCF1"));
            buffer.AddRange(FromUInt32(hardwareType));

            foreach (var pair in this.Variables) {
                if (pair.Key.Equals(".HardwareType", StringComparison.Ordinal)) { continue; } //skip virtual keys
                buffer.AddRange(Encoding.ASCII.GetBytes(pair.Key + "=" + pair.Value + "\0"));
            }

            using (var msOut = new MemoryStream()) {
                using (var gzStream = new GZipStream(msOut, CompressionMode.Compress)) {
                    gzStream.Write(buffer.ToArray(), 0, buffer.Count);
                } //must close stream before returning array (or flush)
                return msOut.ToArray();
            }
        }

        private byte[] GetBytesForDDWrt() {
            var buffer = new List<Byte>();

            buffer.AddRange(Encoding.ASCII.GetBytes("DD-WRT"));
            buffer.AddRange(FromUInt16((ushort)this.Variables.Count));

            foreach (var pair in this.Variables) {
                if (pair.Key.StartsWith("wl_", StringComparison.Ordinal)) { //save wl_ entries first
                    if (pair.Key.Length > 255) { throw new InvalidOperationException("Cannot have key longer than 255 bytes"); }
                    if (pair.Value.Length > 65535) { throw new InvalidOperationException("Cannot have value longer than 65535 bytes"); }
                    buffer.Add((byte)pair.Key.Length);
                    buffer.AddRange(Encoding.ASCII.GetBytes(pair.Key));
                    buffer.AddRange(FromUInt16((ushort)pair.Value.Length));
                    buffer.AddRange(Encoding.ASCII.GetBytes(pair.Value));
                }
            }

            foreach (var pair in this.Variables) {
                if (!pair.Key.StartsWith("wl_", StringComparison.Ordinal)) {
                    if (pair.Key.Length > 255) { throw new InvalidOperationException("Cannot have key longer than 255 bytes"); }
                    if (pair.Value.Length > 65535) { throw new InvalidOperationException("Cannot have value longer than 65535 bytes"); }
                    buffer.Add((byte)pair.Key.Length);
                    buffer.AddRange(Encoding.ASCII.GetBytes(pair.Key));
                    buffer.AddRange(FromUInt16((ushort)pair.Value.Length));
                    buffer.AddRange(Encoding.ASCII.GetBytes(pair.Value));
                }
            }

            return buffer.ToArray();
        }

        private byte[] GetBytesForText() {
            var list = new List<string>();
            foreach (var pair in this.Variables) {
                list.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", EncodeText(pair.Key), EncodeText(pair.Value)));
            }
            list.Sort();

            var sb = new StringBuilder();
            foreach (var item in list) {
                sb.AppendLine(item);
            }

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        #endregion


        #region Helpers

        private static UInt16 ToUInt16(byte[] buffer, int offset) {
            return BitConverter.ToUInt16(buffer, offset);
        }

        private static byte[] FromUInt16(UInt16 value) {
            return BitConverter.GetBytes(value);
        }


        private static UInt32 ToUInt24(byte[] buffer, int offset) {
            var bufferAlt = new byte[4];
            Buffer.BlockCopy(buffer, offset, bufferAlt, 0, 3);
            return BitConverter.ToUInt32(bufferAlt, 0);
        }

        private static byte[] FromUInt24(UInt32 value) {
            var buffer = BitConverter.GetBytes(value);
            return new byte[] { buffer[0], buffer[1], buffer[2] };
        }


        private static UInt32 ToUInt32(byte[] buffer, int offset) {
            return BitConverter.ToUInt32(buffer, offset);
        }

        private static byte[] FromUInt32(UInt32 value) {
            return BitConverter.GetBytes(value);
        }


        private static string ToString(byte[] buffer, int offset, int count) {
            return Encoding.ASCII.GetString(buffer, offset, count);
        }


        internal static string EncodeText(string text) {
            var sb = new StringBuilder();
            foreach (var ch in text) {
                if (ch == '\n') {
                    sb.Append(@"\n");
                } else if (ch == '\r') {
                    sb.Append(@"\r");
                } else if (ch == '\t') {
                    sb.Append(@"\t");
                } else if (ch == '\b') {
                    sb.Append(@"\b");
                } else if (ch == '\f') {
                    sb.Append(@"\f");
                } else if (ch == '\\') {
                    sb.Append(@"\\");
                } else {
                    var value = Encoding.ASCII.GetBytes(new char[] { ch })[0];
                    if ((value < 32) || (value > 127)) {
                        sb.AppendFormat(CultureInfo.InvariantCulture, @"\x{0:X2}", value);
                    } else {
                        sb.Append(ch);
                    }
                }
            }
            return sb.ToString();
        }

        internal static string DecodeText(string text) {
            var sb = new StringBuilder();
            var sbHex = new StringBuilder();

            var state = DTState.Text;
            foreach (var ch in text) {
                switch (state) {
                    case DTState.Text: {
                            if (ch == '\\') {
                                state = DTState.Escape;
                            } else {
                                sb.Append(ch);
                            }
                        } break;

                    case DTState.Escape: {
                            switch (ch) {
                                case 'n': sb.Append("\n"); state = DTState.Text; break;
                                case 'r': sb.Append("\r"); state = DTState.Text; break;
                                case 't': sb.Append("\t"); state = DTState.Text; break;
                                case 'b': sb.Append("\b"); state = DTState.Text; break;
                                case 'f': sb.Append("\f"); state = DTState.Text; break;
                                case '\\': sb.Append(@"\"); state = DTState.Text; break;
                                case 'x': state = DTState.EscapeHex; break;
                                default: throw new FormatException("Invalid escape sequence.");
                            }
                        } break;

                    case DTState.EscapeHex: {
                            sbHex.Append(ch);
                            if (sbHex.Length == 2) {
                                var hex = sbHex.ToString();
                                sbHex.Length = 0;
                                byte value;
                                if (!byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) {
                                    throw new FormatException("Invalid hexadecimal escape sequence.");
                                }
                                sb.Append(Encoding.ASCII.GetString(new byte[] { value }));
                                state = DTState.Text;
                            }
                        } break;
                }
            }

            if (state == DTState.Text) {
                return sb.ToString();
            } else {
                throw new FormatException("Invalid character sequence.");
            }
        }

        private enum DTState {
            Text,
            Escape,
            EscapeHex
        }

        #endregion

    }


    [Flags]
    internal enum NvramFormat {
        AsuswrtVersion1 = 1,
        AsuswrtVersion2 = 2,
        Tomato = 4,
        DDWrt = 8,
        Text = 0x40000000,
        All = 0x7FFFFFFF,
    }
}
