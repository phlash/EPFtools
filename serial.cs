// Serial port playing..
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Collections;
using System.Threading;

public class serial {
	public static void Main(String[] args) {
		string port = "/dev/ttyUSB0";
		if (args.Length > 0)
			port = args[0];
		SerialPort sp = new SerialPort(port, 57600);
		try {
			sp.Open();
			Console.WriteLine("port open, you have the console!");
			Console.WriteLine("Press '5' to enter programming mode and stop the semicolon output, see ICP.txt.");
			Console.WriteLine("Press Escape to quit, Press Backspace to start dump.");
			bool done= false;
			while (!done) {
				while (sp.BytesToRead > 0)
					Console.Write(Convert.ToChar(sp.ReadByte()));
				while (Console.KeyAvailable) {
					ConsoleKeyInfo ck = Console.ReadKey(false);
					if (ck.Key == ConsoleKey.Escape)
						done = true;
					else if (ck.Key == ConsoleKey.Backspace)
						Dump(sp);
					else if (ck.KeyChar == '\u0000')
						continue;
					else
						sp.Write(new char[1] { ck.KeyChar }, 0, 1);
				}
				Thread.Sleep(100);
			}
		} catch (Exception e) {
			Console.WriteLine("\n\nOops: "+e.ToString());
		} finally {
			if (sp.IsOpen)
				sp.Close();
		}
	}

	// Implement EPF011/021 ICP memory dump protocol..
	private static void Dump(SerialPort sp) {
		Console.WriteLine("\nDumping 0000-FFFF: to dump.bin");
		FileStream dump = new FileStream("dump.bin", FileMode.Create);
		sp.ReadTimeout = 5000;	// msecs
		for (int addr=0; addr<65536; addr+=512) {
			// set mode and address
			Console.Write(string.Format("0x{0:X04}: ", addr));
			sp.DiscardInBuffer();
			sp.DiscardOutBuffer();
			byte badr = (byte)(addr>>8);
			byte[] ack = new byte[1] { 85 };
			int recv;
			sp.Write(new byte[2]{ 139, badr }, 0, 2);
			recv = sp.ReadByte();
			if (recv<0)	{	// timeout
				Console.WriteLine("timeout (cmd)");
				return;
			}
			if (139!=(byte)recv) {
				Console.WriteLine("mode ack failed");
				return;
			}
			sp.Write(ack, 0, 1);
			recv = sp.ReadByte();
			if (recv<0) {
				Console.WriteLine("timeout (addr)");
				return;
			}
			if (recv!=badr) {
				Console.WriteLine("addr ack failed");
				return;
			}
			sp.Write(ack, 0, 1);

			// start execution
			sp.Write(new byte[1]{ 140 }, 0, 1);

			// block input, ack'ing each byte
			for (int i=0; i<512; i++) {
				recv = sp.ReadByte();
				if (recv<0) {
					Console.WriteLine("timeout (data)");
					return;
				}
				sp.Write(ack, 0, 1);
				dump.WriteByte((byte)recv);
			}

			// block end and ack
			recv = sp.ReadByte();
			if (recv<0) {
				Console.WriteLine("timeout (block end)");
				return;
			}
			if (136!=(byte)recv) {
				Console.WriteLine("block ack failed");
				return;
			}
			sp.Write(ack, 0, 1);
			Console.WriteLine("OK");
		}
		dump.Close();
		Console.WriteLine("done :)");
	}
}

