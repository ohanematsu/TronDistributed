using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System;

using MiniJSON;

/**
 *  NetworkManager - a class that communicates with the game logic processing unit via 
 * 					 json formatted message
 */
public class NetworkManager : MonoBehaviour {

	// Fields
	internal bool mSocketReady = false;
	private string mIpAddr = "128.237.209.209";
	private int mPort = 18843;
	private string mUserID = "";
	private TcpClient mClientSocket;
	private NetworkStream mStream;
	private StreamWriter mWriter;
	private StreamReader mReader;
	private Thread mConnThread;

	private Queue mMsgQueue = new Queue();

	// For debugging message exchange
	void OnGUI() {
		if (!mSocketReady) {
			GUI.Label (new Rect (10, 10, 200, 20), "Status: Disconnected");
		}
		else {
			GUI.Label (new Rect (10, 10, 200, 20), "Status: Connected");
		}

		GUI.Label (new Rect (200, 10, 200, 20), "Queue count: " + mMsgQueue.Count.ToString());

		/* 
		 * Event Simulation for debugging
		 */
		if (GUI.Button(new Rect(10, 110, 120, 20), "Read Message"))
		{
			// Print the received message
			/*Message msg = receive();

			if(msg != null)
			{
				msg.printMessage();
			}
			else
			{
				Debug.Log("There are no more messages in the queue");
			}*/
		}
		if (GUI.Button(new Rect(10, 90, 120, 20), "Send Message"))
		{
			Debug.Log("Sending message....");
			//string msg = "{\"Name\":\"test\", \"array\":[1,{\"data\":\"value\"}]}";
			//string msg = "{\"Type\":\"test\", \"UserName\":5, \"VerticalDir\":1.5, \"HorizontalDir\":5.8, \"PosX\":3.0, \"PosY\":6.0, \"PosZ\":10.0}";
			//Debug.Log ("msg test: " + msg);
			//Message m = new Message (mUserID, "test1", new Vector3 (1.54f, 4.37f, 10.59f), 5.0f, 10.0f, 34, new Vector3(2.34f, 3.45f, 4.56f), new Quaternion(1.11f, 2.22f, 3.33f, 4.44f));
			//Debug.Log ("real message str: " + m.toJsonString());
			//writeSocket(msg);
			//writeSocket(m.toJsonString());
		}

		if (GUI.Button(new Rect(10, 50, 160, 20), "Establish Connection"))
		{
			// Initialize the connection to the game logic componenet
			initialize();
		}

		if (GUI.Button(new Rect(10, 70, 160, 20), "Disconnect"))
		{
			// Close the connection to the game logic componenet
			closeSocket();
		}

	}

	// This function is invoked to initialize variables, stuff, etc
	void Start() 
	{
		// Dummy function
		mIpAddr = LocalIPAddress();

		// May there are other stuffs that need to be initialized?
		mUserID = mIpAddr + ":18842";
	}

	// This is where the socket is setup and the connthread is established
	public void initialize()
	{
		Debug.Log("Initializing....");
		
		// Establish the connection with the game message processing component
		int ret = setupSocket();

		if(ret == 0)
		{
			// Start the msg receiving thread
			this.mConnThread = new Thread(new ThreadStart(handleConn));
			this.mConnThread.Start();

			// Initialize message queue
			//mMsgQueue = new Queue();
		}
		else
		{
			Debug.Log("Initialization error - Setup Socket failed");
		}
	}

	// This is the actual implementation of establishing connection
	public int setupSocket() 
	{ 
		int result = 0;

		try {
			Debug.Log("Set up socket");
			mClientSocket = new TcpClient(mIpAddr, mPort);
			mStream = mClientSocket.GetStream(); 
			mWriter = new StreamWriter(mStream);
			mReader = new StreamReader(mStream);
			mSocketReady = true;
			Debug.Log("Done set up socket");
			result = 0;
		}
		catch (Exception e) {
			Debug.Log("Socket error: " + e);
			result = -1;
		}

		return result;
	}

	// Send the message over the output stream of the established socket
	public void writeSocket(object v) {
		if (!mSocketReady)
			return;
		mWriter.Write(Json.Serialize(v));
		mWriter.Flush();
		
	}

	// Threaded function that handles events of receiving message
	public void handleConn() {
		if (!mSocketReady) {
			return;	
		}
						 
		// More to be done in this loop?
		// Like putting the message in queue?
		while (true) {
			//Message recved_msg = new Message(message);
			//recved_msg.printMessage();
			var N = Json.Deserialize(mReader.ReadLine());
			if (N == null) continue;
			Debug.Log ("recved message: " + N);

			//Debug.Log ("Enqueuing received message...");
			mMsgQueue.Enqueue(N);
		}
	}

	// Dequeue the message received from the game logic, if any, and return it to the 
	// caller
	public Dictionary<string, object> receive()
	{
		object dq_msg = null;

		if(mMsgQueue.Count > 0)
		{
			Debug.Log("Dequeueing message....");
			dq_msg = mMsgQueue.Dequeue();
		}

		return dq_msg as Dictionary<string, object>;
	}

	// Close the connection
	public void closeSocket() {
		if (!mSocketReady)
			return;

		mWriter.Close();
		mReader.Close();
		mClientSocket.Close();
		mSocketReady = false;
		mConnThread.Abort();
	}

	public bool GetSocketState() {
		return mSocketReady;
	}

	public string GetUserID() {
		return mUserID;
	}

	public string LocalIPAddress()
	{
		IPHostEntry host;
		string localIP = "";
		
		/* Use TCP socket connection to retrieve local IP address */
		TcpClient tron_server_sock = new TcpClient("team18.ece842.com", 80);
		localIP = ((IPEndPoint)tron_server_sock.Client.LocalEndPoint).Address.ToString ();
		
		Debug.Log ("localIP: " + localIP);
		
		if (tron_server_sock.Connected) 
		{
			tron_server_sock.Close();
			Debug.Log ("tron_server_sock closed");
		}
		
		/* Use DNS to resolve for local IP address if localIP is 127.0.0.1 */
		if (localIP.Equals ("127.0.0.1")) 
		{
			Debug.Log("localIp is 127.0.0.1!!!");
			
			Debug.Log ("Using Dns.GetHostByName()....");
			//host = Dns.GetHostByName(Dns.GetHostName());
			host = Dns.GetHostEntry(Dns.GetHostName());
			Debug.Log ("Dns.GetHostName(): " + Dns.GetHostName ());
			foreach (IPAddress ip in host.AddressList)
			{
				Debug.Log("ip addr: " + ip.ToString());
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					Debug.Log("InterNetwork ip addr: " + ip.ToString());
					localIP = ip.ToString();
					break;
				}
			}
			
		}
		
		return localIP;
	}
}
