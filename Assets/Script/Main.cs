using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

	public JFSocket mJFsorket;

	private float mSynchronous;

	// Use this for initialization
	void Start () {
		mJFsorket = JFSocket.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {

		mSynchronous += Time.deltaTime;
		//在Update中每0.5s的时候同步一次
		if(mSynchronous > 0.5f)
		{
			while(true)
			{
				JFSocket.stMsg _msg = mJFsorket.PopRecvMsg();
				if(_msg.size == 0)
					break;

				LoginUserCmd.stNullUserCmd cmd = new LoginUserCmd.stNullUserCmd();
				cmd = (LoginUserCmd.stNullUserCmd)JFSocket.BytesToStruct(_msg.buffer, cmd.GetType());

				Debug.Log("receive one cmd. cmd : " + cmd.byCmd + ", param : " + cmd.byParam);
			}

			mSynchronous = 0;
		}
	}
}
