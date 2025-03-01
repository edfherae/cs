using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _1
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			//Server server = new();
			LogInForm LogIn = new();
			Application.Run(LogIn); //�������� � ����������� ����� ������ ������� ��� �����������
		}
	}
	public class Server
	{
		public string Name { get; private set; }
		public IPEndPoint IPEnd { get; private set; }
		public Socket Socket { get; set; }
		CancellationToken CaT { get; set; }
		public class Message
		{
			string User { get; set; }
			string Text { get; set; }
			//DateTime
			public Message(string user, string text)
			{
				User = user; Text = text;
			}
		}
		public class Chat
		{
			public string[] UsersLogin { get; set; } = new string[10];
			public List<Message> Messages { get; set; } = new List<Message>();
			List<Message> GetMessages(string login)
			{
				if (UsersLogin.Contains(login)) return Messages;
				else return null;
			}
			public Chat(string[] participants)
			{
				UsersLogin = participants;
			}
		}
		Dictionary<string, List<Chat>> userChats { get; set; } = new();

		

		public Server(string name, string address, int port) //���� ��� ���� ���� ������ �� ����� � ��� �� ������
		{
			Name = name;
			IPEnd = new IPEndPoint(IPAddress.Parse(address), port);
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp or ip
			Socket.Bind(IPEnd);

			List<Chat> testChat = new();
			testChat.Add(new Chat(["edfherae", "noname"]));
			userChats.Add("edfherae", testChat);
			//MessageLoopTask = new Task(MessageLoop);
		}
		//public void CreateChat(string login);
		//public Chat GetChat(string login)

		//�������� ��� �������� �����������, ���������� � ��� ������� ����������� �������� ���� ���������
		public static async Task StartServer(Server server, CancellationToken ct)
		{
			server.Socket.Listen();
			Console.WriteLine("������ �������. �������� �����������...");
			while(!ct.IsCancellationRequested)
			{
				Socket client = await server.Socket.AcceptAsync();
				Console.WriteLine($"��������� ������ {client.RemoteEndPoint}");
				_ = ClientHandle(client, server);
			}
			Console.WriteLine("������ ��������.");
			//userSocket.Send(Encoding.UTF8.GetBytes($"Hello, {userSocket.RemoteEndPoint}!\n"));
			//server.Socket.BeginAccept(BeginAcceptCallback, server.Socket);
		}
		enum Commands 
		{
			get_user_chats,
			error = 404
		}
		enum Answers
		{ 
			send_user_chats,
			error = 404
		}
		static async Task ClientHandle(Socket client, Server server)
		{
			//ClientHandle �������� ���������� � ����� � ������������� ��
			string message = "";
			string userLogin = "";

			byte[] temp = new byte[1024];
			//������ ���������� ������ ��������� ����� (� ������)
			//������ �� ���� ������������ �������

			//get_user_chats\0
			//send_user_chats;List<Chat>\0

			await client.ReceiveAsync(temp); //�������� �����
			userLogin = Encoding.UTF8.GetString(temp);

			//���������� �� �����: �����, ���������� �����, ���������
			//client.SendAsync(buffer);

			

			while (client.Connected)
			{
				byte[] buffer = new byte[1024];
				await client.ReceiveAsync(buffer);
				message = Encoding.UTF8.GetString(buffer);
				switch(message)
				{
					case "get_user_chats":
						{
							List<Chat> userChats;
							if(server.userChats.TryGetValue(userLogin, out userChats))
							{
								string chatList = "";
								for (int i = 0; i < userChats.Count; i++)
								{
									for(int j=0;j< userChats[i].UsersLogin.Length;j++)
									{
										chatList += userChats[i].UsersLogin[j] + "\n";
									}
								}
								await client.SendAsync(Encoding.UTF8.GetBytes(Answers.send_user_chats.ToString() + chatList + "\0")); 
							}
							else await client.SendAsync(Encoding.UTF8.GetBytes(Answers.error.ToString()));
							break;
						}
				}

			}
			//userSocket.Send(Encoding.UTF8.GetBytes("\r\n������� �����:"));

			//while (ct.IsCancellationRequested)
			//{
			//	userSocket.Receive(buffer);
			//	userSocket.Send(Encoding.UTF8.GetBytes("\r\n"));
			//	userSocket.Send(buffer);
			//}
			//client.Send(Encoding.UTF8.GetBytes("Bye!\r\n"));
			client.Shutdown(SocketShutdown.Both);
			client.Close();
		}
	}
}