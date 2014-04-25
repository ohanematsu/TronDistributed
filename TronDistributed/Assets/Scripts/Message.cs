using UnityEngine;
using System.Collections;
using SimpleJSON;

/**
 * Message - a class that defines the data unit transimitted between
 * 			 the game processing unit and the game interface
 */
public class Message {

	// Fields
	private string mType = "";
	private string mUserID = "";
	private Vector3 mPosition;
	private float mVerticDir;
	private float mHoriDir;

	public int mTime;
	public Vector3 mMovement;
	public Quaternion mRotation;

	// Default Constructor
	public Message()
	{
		mUserID = "";
		mPosition = Vector3.zero;
		mVerticDir = 0.0f;
		mHoriDir = 0.0f;
		mTime = 0;
		mMovement = Vector3.zero;
		mRotation = new Quaternion (0f, 0f, 0f, 0f);
	}

	// Generic Constructor
	public Message(string userID, string type, Vector3 pos, float verti, float hori, int time, Vector3 mov, Quaternion rot)
	{
		mUserID = userID;
		mType = type;
		mPosition = new Vector3 (pos.x, pos.y, pos.z);
		mVerticDir = verti;
		mHoriDir = hori;
		mTime = time;
		mMovement = new Vector3 (mov.x, mov.y, mov.z);
		mRotation = new Quaternion (rot.x, rot.y, rot.z, rot.w);
	}

	// Constructor based on Json string
	public Message(string jsonString)
	{
		var N = JSONNode.Parse(jsonString);

		mUserID = N ["UserID"];
		mType = N["Type"];
		float pos_x = N ["PosX"].AsFloat;
		float pos_y = N ["PosY"].AsFloat;
		float pos_z = N ["PosZ"].AsFloat;
		mPosition = new Vector3(pos_x, pos_y, pos_z);
		mVerticDir = N ["VerticalDir"].AsFloat;
		mHoriDir = N ["HorizontalDir"].AsFloat;

		float mov_x = N["MovX"].AsFloat;
		float mov_y = N["MovY"].AsFloat;
		float mov_z = N["MovZ"].AsFloat;

		mTime = N["Time"].AsInt;
		mMovement = new Vector3 (mov_x, mov_y, mov_z);

		float rot_x = N["RotX"].AsFloat;
		float rot_y = N["RotY"].AsFloat;
		float rot_z = N["RotZ"].AsFloat;
		float rot_w = N["RotW"].AsFloat;

		mRotation = new Quaternion (rot_x, rot_y, rot_z, rot_w);
	}

	// Setters
	public void setUserName(string userID)
	{
		mUserID = userID;
	}

	public void setPosition(Vector3 pos)
	{
		mPosition = new Vector3(pos.x, pos.y, pos.z);
	}

	public void setType(string type)
	{
		mType = type;
	}

	public void setVerticalDir(float vertiDir)
	{
		mVerticDir = vertiDir;
	}

	public void setHorizontalDir(float horiDir)
	{
		mHoriDir = horiDir;
	}

	// Getters
	public string getUserID()
	{
		return mUserID;
	}

	public Vector3 getPosition()
	{
		return mPosition;
	}

	public float getVerticalDir()
	{
		return mVerticDir;
	}

	public float getHorizontalDir()
	{
		return mHoriDir;
	}

	public string getType()
	{
		return mType;
	}

	// Converts the data fields of a message into json string so
	// it can be transmitted in json format
	public string toJsonString()
	{
		// This json format cannot be recognized by golang json package
		// wtf?
		// This package's parser doesn't work well with golang's json package
//		var json_string = new JSONClass();
//		json_string["UserName"].AsInt = mUserName;
//		json_string["Type"] = mType;
//		json_string["VerticalDir"].AsFloat = mVerticDir;
//		json_string["HorizontalDir"].AsFloat = mHoriDir;
//		json_string["PosX"].AsFloat = mPosition.x;
//		json_string["PosY"].AsFloat = mPosition.y;
//		json_string["PosZ"].AsFloat = mPosition.z;
//		return json_string.ToString();

		 //string msg = "{\"Type\":\"test\", \"UserName\":5, \"VerticalDir\":1.5, \"HorizontalDir\":5.8, \"PosX\":3.0, \"PosY\":6.0, \"PosZ\":10.0}";

		// Manual conversion
		string result = "";

		string double_quote = "\"";
		string type_field = "Type";
		string userName_field = "UserName";
		string verti_field = "VerticalDir";
		string hori_field = "HorizontalDir";
		string posx_filed = "PosX";
		string posy_filed = "PosY";
		string posz_filed = "PosZ";

		string mov_x_field = "MovX";
		string mov_y_field = "MovY";
		string mov_z_field = "MovZ";
		
		string time_field = "Time";
		
		string rot_x_field = "RotX";
		string rot_y_field = "RotY";
		string rot_z_field = "RotZ";
		string rot_w_field = "RotW";

		// Convert the message fields to json string
		result += "{";
		result += (double_quote + type_field + double_quote + ":" + double_quote + mType + double_quote + ", ");
		result += (double_quote + userName_field + double_quote + ":" + double_quote + mUserID + double_quote + ", ");
		result += (double_quote + verti_field + double_quote + ":" + mVerticDir + ", ");
		result += (double_quote + hori_field + double_quote + ":" + mHoriDir.ToString("0.00") + ", ");
		result += (double_quote + posx_filed + double_quote + ":" + mPosition.x + ", ");
		result += (double_quote + posy_filed + double_quote + ":" + mPosition.y + ", ");
		result += (double_quote + posz_filed + double_quote + ":" + mPosition.z + ", ");

		result += (double_quote + time_field + double_quote + ":" + mTime + ", ");
		result += (double_quote + mov_x_field + double_quote + ":" + mMovement.x + ", ");
		result += (double_quote + mov_y_field + double_quote + ":" + mMovement.y + ", ");
		result += (double_quote + mov_z_field + double_quote + ":" + mMovement.z + ", ");

		result += (double_quote + rot_x_field + double_quote + ":" + mRotation.x + ", ");
		result += (double_quote + rot_y_field + double_quote + ":" + mRotation.y + ", ");
		result += (double_quote + rot_z_field + double_quote + ":" + mRotation.z + ", ");
		result += (double_quote + rot_w_field + double_quote + ":" + mRotation.w);
		result += "}";

		return result;
	}

	// Print/Debug the message content
	public void printMessage()
	{
		Debug.Log ("msg var\nUserName: " + mUserID + "\nType: " + mType + "\nVertical Dir: " + 
		           mVerticDir.ToString("0.00") + "\nHorizontal Dir: " + mHoriDir.ToString("0.00") + 
		           "\nPosition: " + mPosition.ToString("0.00") + "\nTime: " + mTime.ToString() + 
		           "\nMovement: " + mMovement.ToString("0.00") + "\nRotation: " + mRotation.ToString("0.00"));
	}

}
