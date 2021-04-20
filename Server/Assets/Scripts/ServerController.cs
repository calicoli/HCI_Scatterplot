using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerController : MonoBehaviour {

	public Text ipText;
	public GameObject obj;
	public GameObject touchProcessor;
    
	public Camera renderCamera;
    public GameObject selectVisulizer;
    public GameObject filterProcessor;

	private Color disconnectColor = new Color(0.8156f, 0.3529f, 0.4313f);
	//private Color connectColor = new Color(0.5254f, 0.7568f, 0.4f);
	private Color connectColor = new Color(0f, 0f, 0f);

	private Vector3 pos = new Vector3(0, 0, 0);

	private TcpListener tcpListener;
	private Thread tcpListenerThread;
	private TcpClient connectedTcpClient;
	private string rcvMsg = "";
	private bool refreshed = false;

	private bool noConnection = true;
	
	void Start () {
		tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
	}
	
	void Update () {
		ipText.text = getIPAddress();
        
		renderCamera.backgroundColor = (connectedTcpClient == null ? disconnectColor : connectColor);
		if (connectedTcpClient != null && noConnection) {
			sendMessage();
			noConnection = false;
		}
		if (refreshed) {
			getVector();
			refreshed = false;
		}
	}
	
	private void ListenForIncommingRequests () {
		try {
			tcpListener = new TcpListener(IPAddress.Any, 8052);
			tcpListener.Start();
			Debug.Log("Server is listening");
			Byte[] bytes = new Byte[1024];
			while (true) {
				using (connectedTcpClient = tcpListener.AcceptTcpClient()) {
					using (NetworkStream stream = connectedTcpClient.GetStream()) {
						int length;
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							rcvMsg = Encoding.ASCII.GetString(incommingData);
							refreshed = true;
						}
					}
				}
			}
		}
		catch (SocketException socketException) {
			Debug.Log("SocketException " + socketException.ToString());
		}
	}
	
	public void sendMessage() {
		Vector3 tp = touchProcessor.GetComponent<TouchProcessor>().pos;
        //Vector3 tv = faceTracker.GetComponent<FaceTracker>().currentObserve;
        //Vector3 acc = Input.acceleration;
        Quaternion tr = touchProcessor.GetComponent<TouchProcessor>().rot;
        Vector3 ts = touchProcessor.GetComponent<TouchProcessor>().sca;
        char tCharMode = touchProcessor.GetComponent<TouchProcessor>().charMode;
        bool tisServerFiltering = touchProcessor.GetComponent<TouchProcessor>().isFilteringInServer;
        if (connectedTcpClient == null) {
			return;
		}
		
		try {			
			NetworkStream stream = connectedTcpClient.GetStream();
			if (stream.CanWrite) {
                string serverMessage =
                    tp.x + "," + tp.y + "," + tp.z + "," +
                    tr.x + "," + tr.y + "," + tr.z + "," + tr.w + "," +
                    ts.x + "," + ts.y + "," + ts.z + "," +
                    tCharMode + "," +
                    (tisServerFiltering ? "T" : "F") + ",";
                    //(tModeSelect ? "T" : "F") + ",";
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
				Debug.Log("Server sent his message - should be received by client");
			}
		}
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);
		}
	}
	
	private string getIPAddress() {
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new System.Exception("No network adapters with an IPv4 address in the system!");
	}

	private void getVector() {
		string[] temp = rcvMsg.Split(',');
        
		if (!touchProcessor.GetComponent<TouchProcessor>().isLocked) {
            // pan
			float x = System.Convert.ToSingle(temp[0]);
			float y = System.Convert.ToSingle(temp[1]);
			float z = System.Convert.ToSingle(temp[2]);
			Vector3 v = new Vector3(x, y, z);
			touchProcessor.GetComponent<TouchProcessor>().pos = v;
            // rotate
            touchProcessor.GetComponent<TouchProcessor>().rot.x = System.Convert.ToSingle(temp[3]);
            touchProcessor.GetComponent<TouchProcessor>().rot.y = System.Convert.ToSingle(temp[4]);
            touchProcessor.GetComponent<TouchProcessor>().rot.z = System.Convert.ToSingle(temp[5]);
            touchProcessor.GetComponent<TouchProcessor>().rot.w = System.Convert.ToSingle(temp[6]);
            // scale
            touchProcessor.GetComponent<TouchProcessor>().sca =
                new Vector3(System.Convert.ToSingle(temp[7]),
                            System.Convert.ToSingle(temp[8]),
                            System.Convert.ToSingle(temp[9]));
        }
        if(touchProcessor.GetComponent<TouchProcessor>().getCurrentMode() == TouchProcessor.Mode.selectR)
        {
            selectVisulizer.GetComponent<SelectVisualizer>().isSelectingInClient =
                (temp[10][0] == 'T' ? true : false);
            selectVisulizer.GetComponent<SelectVisualizer>().selectOffset =
                System.Convert.ToSingle(temp[11]);
            selectVisulizer.GetComponent<SelectVisualizer>().selectDepth =
                System.Convert.ToSingle(temp[12]);
        }
        if (touchProcessor.GetComponent<TouchProcessor>().getCurrentMode() == TouchProcessor.Mode.filter1 ||
            touchProcessor.GetComponent<TouchProcessor>().getCurrentMode() == TouchProcessor.Mode.filter2)
        {
            bool clientFiltering = (temp[13][0] == 'T') ? true : false;
            float minn = System.Convert.ToSingle(temp[14]);
            float maxx = System.Convert.ToSingle(temp[15]);
            //filterProcessor.GetComponent<FilterProcessor>().ProcessorClientRange(
            //    clientFiltering, minn, maxx);
            touchProcessor.GetComponent<TouchProcessor>().processClientFilterRange(clientFiltering, minn, maxx);
        }
        if (touchProcessor.GetComponent<TouchProcessor>().getCurrentMode() == TouchProcessor.Mode.selectT ||
            touchProcessor.GetComponent<TouchProcessor>().getCurrentMode() == TouchProcessor.Mode.selectD ||
            touchProcessor.GetComponent<TouchProcessor>().getCurrentMode() == TouchProcessor.Mode.selectA )
        {
            int cntClientTetra = System.Convert.ToInt32(temp[16]);
            Vector2 tp1 = new Vector2(System.Convert.ToSingle(temp[17]), System.Convert.ToSingle(temp[18]));
            Vector2 tp2 = new Vector2(System.Convert.ToSingle(temp[19]), System.Convert.ToSingle(temp[20]));
            touchProcessor.GetComponent<TouchProcessor>().
                processClientTetraTouch(cntClientTetra, tp1, tp2);
        }

        //touchProcessor.GetComponent<TouchProcessor>().angle = System.Convert.ToSingle(temp[3]);

    }

}