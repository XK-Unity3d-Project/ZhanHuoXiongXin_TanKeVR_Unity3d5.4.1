﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerAmmoType
{
	Null = -1,
	PuTongAmmo,
	GaoBaoAmmo,
	DaoDanAmmo,
}

public class XKPlayerAutoFire : MonoBehaviour {
	
	public LayerMask FireLayer;
	public TweenRotation[] QianGuanTwRot;
	public Transform[] AmmoStartPosOne;
	public Transform[] AmmoStartPosTwo;
	public Transform[] DaoDanAmmoPosOne;
	public Transform[] DaoDanAmmoPosTwo;
	public GameObject[] AmmoParticle;
	public GameObject[] GaoBaoAmmoParticle;
	public GameObject[] DaoDanAmmoParticle;
	public GameObject PuTongAmmo;
	public GameObject DaoDanAmmo;
	public GameObject GaoBaoDanAmmo;
	[Range(1f, 500f)] public float Frequency = 10f; //普通子弹发射频率.
	[Range(1f, 500f)] public float FrequencyGaoBao = 10f; //高爆弹发射频率.
	[Range(1f, 500f)] public float DaoDanTimeMin = 1.5f; //导弹冷却时间.
	[Range(0f, 1f)] public float DRPTFireVolume = 1f; //单人开枪音量.
	[Range(0f, 1f)] public float SRPTFireVolume = 0.5f; //双人开枪音量.
	[Range(0f, 1f)] public float DRGBFireVolume = 1f; //单人开枪音量.
	[Range(0f, 1f)] public float SRGBFireVolume = 0.5f; //双人开枪音量.
	bool IsActiveFireBtOne;
	bool IsActiveFireBtTwo;
	float LastFireTimeOne = -1;
	float LastFireTimeTwo = -1;
	public static PlayerAmmoType AmmoStatePOne = PlayerAmmoType.PuTongAmmo;
	public static PlayerAmmoType AmmoStatePTwo = PlayerAmmoType.PuTongAmmo;
	float OffsetForward = 30f;
	float FirePosValTmp = 1000f;
	float FireRayDirLen = 500f;
	public static int MaxAmmoCount = 30;
	float[] DaoDanTimeVal = {0f, 0f};
	public static List<PlayerAmmoCtrl> AmmoList; //普通子弹.
	public static List<PlayerAmmoCtrl> AmmoGaoBaoList; //高爆子弹.
	public static List<PlayerAmmoCtrl> AmmoList_TK; //普通子弹.
	public static List<PlayerAmmoCtrl> AmmoGaoBaoList_TK; //高爆子弹.
	XkPlayerCtrl PlayerScript;
	/**
PlayerAudio[0] -> p1主角飞机/坦克机枪开枪音效.
PlayerAudio[1] -> p1主角飞机/坦克打高爆弹音效.
PlayerAudio[2] -> p1主角飞机/坦克发射导弹音效.
PlayerAudio[3] -> p2主角飞机/坦克机枪开枪音效.
PlayerAudio[4] -> p2主角飞机/坦克打高爆弹音效.
PlayerAudio[5] -> p2主角飞机/坦克发射导弹音效.
PlayerAudio[6] -> 主角飞机/坦克行驶音效.
	*/
	AudioSource[] PlayerAudio;
	bool IsPSAutoFire;
	public static bool IsAimPlayerPOne;
	public static bool IsAimPlayerPTwo;
	PlayerAmmoType PSAmmoTypeVal = PlayerAmmoType.Null;
	float TimeAimPlayerPOne;
	float TimeAimPlayerPTwo;
	PlayerTypeEnum PlayerStEnum;
	// Use this for initialization
	void Start()
	{
		//AmmoParticleList = new List<AmmoParticleDt>(6);
		for (int i = 0; i < QianGuanTwRot.Length; i++) {
			QianGuanTwRot[i].enabled = false;
		}
		FireLayer = XkGameCtrl.GetInstance().PlayerAmmoHitLayer;
		PlayerScript = GetComponent<XkPlayerCtrl>();
		PlayerScript.SetPlayerAutoFireScript(this);
		PlayerStEnum = PlayerScript.PlayerSt;

		AmmoStatePOne = PlayerAmmoType.PuTongAmmo;
		AmmoStatePTwo = PlayerAmmoType.PuTongAmmo;
		if (PlayerStEnum != PlayerTypeEnum.CartoonCamera) {
			PlayerAudio = PlayerScript.PlayerAudio;
			for (int i = 0; i < 7; i++) {
				if (i < 6) {
					PlayerAudio[i].loop = false;
				}
				else {
					PlayerAudio[i].loop = true;
				}
				PlayerAudio[i].Stop();
			}
			
			InitPlayerAmmoList();
			InputEventCtrl.GetInstance().ClickFireBtOneEvent += ClickFireBtOneEvent;
			InputEventCtrl.GetInstance().ClickFireBtTwoEvent += ClickFireBtTwoEvent;
			InputEventCtrl.GetInstance().ClickDaoDanBtOneEvent += ClickDaoDanBtOneEvent;
			InputEventCtrl.GetInstance().ClickDaoDanBtTwoEvent += ClickDaoDanBtTwoEvent;
//			if (Network.peerType == NetworkPeerType.Server) {
//				InitPlayerAmmoList();
//			}
//
//			if ((PlayerStEnum == PlayerTypeEnum.FeiJi && XkGameCtrl.GameJiTaiSt == GameJiTaiType.FeiJiJiTai)
//			    || (PlayerStEnum == PlayerTypeEnum.TanKe && XkGameCtrl.GameJiTaiSt == GameJiTaiType.TanKeJiTai)){
//				InitPlayerAmmoList();
//				InputEventCtrl.GetInstance().ClickFireBtOneEvent += ClickFireBtOneEvent;
//				InputEventCtrl.GetInstance().ClickFireBtTwoEvent += ClickFireBtTwoEvent;
//				InputEventCtrl.GetInstance().ClickDaoDanBtOneEvent += ClickDaoDanBtOneEvent;
//				InputEventCtrl.GetInstance().ClickDaoDanBtTwoEvent += ClickDaoDanBtTwoEvent;
//			}
		}
		else {
			this.enabled = false;
		}
	}

	void InitPlayerAmmoList()
	{
		if (AmmoList != null) {
			AmmoList.Clear();
		}
		AmmoList = new List<PlayerAmmoCtrl>(MaxAmmoCount); //普通子弹.
		
		if (AmmoGaoBaoList != null) {
			AmmoGaoBaoList.Clear();
		}
		AmmoGaoBaoList = new List<PlayerAmmoCtrl>(MaxAmmoCount); //高爆子弹.

		if (AmmoList_TK != null) {
			AmmoList_TK.Clear();
		}
		AmmoList_TK = new List<PlayerAmmoCtrl>(MaxAmmoCount); //普通子弹.

		if (AmmoGaoBaoList_TK != null) {
			AmmoGaoBaoList_TK.Clear();
		}
		AmmoGaoBaoList_TK = new List<PlayerAmmoCtrl>(MaxAmmoCount); //高爆子弹.
	}

	// Update is called once per frame
	void Update()
	{
//		if (XKTriggerClosePlayerUI.IsClosePlayerUI) {
//			return;
//		}

		if (PlayerStEnum == PlayerTypeEnum.CartoonCamera) {
			return;
		}
		CheckPlayerOneFireBt();
		CheckPlayerTwoFireBt();
		CheckPSTriggerAutoFire();

		//检测队友碰撞提示.
//		if (XkGameCtrl.GameModeVal == GameMode.LianJi) {
//			if (!XKTriggerClosePlayerUI.IsActiveHeTiCloseUI) {
//				CheckCrossAimObjPlayerOne();
//				CheckCrossAimObjPlayerTwo();
//			}
//			else {
//				if (IsAimPlayerPOne) {
//					IsAimPlayerPOne = false;
//				}
//				
//				if (IsAimPlayerPTwo) {
//					IsAimPlayerPTwo = false;
//				}
//			}
//		}
	}

	GameObject SpawnPlayerAmmo(GameObject ammoPrefab, Vector3 ammoPos, Quaternion ammoRot)
	{
		return (GameObject)Instantiate(ammoPrefab, ammoPos, ammoRot);
	}

	void SetQianGuanTwRot(PlayerEnum indexPlayer, bool isEnable)
    {
        if (QianGuanTwRot == null || QianGuanTwRot.Length < 2) {
            return;
        }

        int indexVal = (int)indexPlayer - 1;
        if (QianGuanTwRot[indexVal].enabled != isEnable) {
			QianGuanTwRot[indexVal].enabled = isEnable;
		}
	}

	GameObject[] AmmoParticleObj = new GameObject[2];
	void CheckPlayerOneFireBt()
	{
		if (XKPlayerCamera.IndexPlayerNum != 0) {
			return;
		}

		if (!ScreenDanHeiCtrl.IsStartGame) {
			SetQianGuanTwRot(PlayerEnum.PlayerOne, false);
			return;
		}
		
		if (!XkGameCtrl.IsActivePlayerOne) {
			SetQianGuanTwRot(PlayerEnum.PlayerOne, false);
			return;
		}

		if (!IsActiveFireBtOne) {
			SetQianGuanTwRot(PlayerEnum.PlayerOne, false);
			return;
		}
		SetQianGuanTwRot(PlayerEnum.PlayerOne, true);

//		if (DaoJiShiCtrl.GetInstance().GetIsPlayDaoJishi()) {
//			return;
//		}

		if (!XkGameCtrl.IsTiaoGuoStartCartoon) {
			if (!ZhunXingCtrl.GetInstanceOne().GetActiveZhunXing() || !ZhunXingTeXiaoCtrl.IsOverTeXiaoZhunXing) {
				return;
			}
		}

		if (Camera.main == null) {
			return;
		}
//		if (GameOverCtrl.IsShowGameOver || JiFenJieMianCtrl.GetInstance().GetIsShowFinishTask()) {
//			IsActiveFireBtOne = false;
//			return;
//		}

//		if (PlayerStEnum == PlayerTypeEnum.FeiJi && XkGameCtrl.GameJiTaiSt != GameJiTaiType.FeiJiJiTai) {
//			IsActiveFireBtOne = false;
//			return;
//		}

//		if (PlayerStEnum == PlayerTypeEnum.TanKe && XkGameCtrl.GameJiTaiSt != GameJiTaiType.TanKeJiTai) {
//			IsActiveFireBtOne = false;
//			return;
//		}

		if (XkGameCtrl.GaoBaoDanNumPOne <= 0
		    && Time.time < LastFireTimeOne + 1f / Frequency) {
			return;
		}
		
		if (XkGameCtrl.GaoBaoDanNumPOne > 0
		    && Time.time < LastFireTimeOne + 1f / FrequencyGaoBao) {
			return;
		}
		LastFireTimeOne = Time.time;
		Vector3 ammoSpawnPos = AmmoStartPosOne[0].position;
		GameObject obj = null;
		CheckFireAudioPlayerOne();
		
		bool isSpawnGaoBaoDan = false;
		if (XkGameCtrl.GaoBaoDanNumPOne <= 0) {
			if (AmmoParticle[0] != null && AmmoParticleObj[0] == null) {
				obj = (GameObject)Instantiate(AmmoParticle[0], ammoSpawnPos, AmmoStartPosOne[0].rotation);
				obj.transform.parent = AmmoStartPosOne[0];
				AmmoParticleObj[0] = obj;
				XkGameCtrl.CheckObjDestroyThisTimed(obj);
			}
			obj = GetPlayerAmmo(PlayerAmmoType.PuTongAmmo, ammoSpawnPos, AmmoStartPosOne[0].rotation);
		}
		else {
			isSpawnGaoBaoDan = true;
			if (GaoBaoAmmoParticle[0] != null) {
				obj = (GameObject)Instantiate(GaoBaoAmmoParticle[0], ammoSpawnPos, AmmoStartPosOne[0].rotation);
				obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
				XkGameCtrl.CheckObjDestroyThisTimed(obj);
			}
			XkGameCtrl.GetInstance().SubGaoBaoDanNumPOne();
			obj = GetPlayerAmmo(PlayerAmmoType.GaoBaoAmmo, ammoSpawnPos, AmmoStartPosOne[0].rotation);
		}
		obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
		PlayerAmmoCtrl ammoScript = obj.GetComponent<PlayerAmmoCtrl>();
		ammoScript.SetIsDonotHurtNpc(false);
		
		Vector3 mousePosInput = Input.mousePosition;
		if (pcvr.bIsHardWare) {
			mousePosInput = pcvr.CrossPositionOne;
		}
		
		Vector3 firePos = Vector3.zero;
		Vector3 mousePos = mousePosInput + Vector3.forward * OffsetForward;
		Vector3 posTmp = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 ammoForward = Vector3.Normalize( posTmp - ammoSpawnPos );
		Ray ray = Camera.main.ScreenPointToRay(mousePosInput);
		RaycastHit hit;
		if (!IsPSAutoFire) {
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ray, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				if (ammoScript.AmmoType == PlayerAmmoType.PuTongAmmo) {
					XKNpcHealthCtrl healthScript = hit.collider.GetComponent<XKNpcHealthCtrl>();
					if (healthScript != null && !healthScript.GetIsDeathNpc()) {
						healthScript.OnDamageNpc(ammoScript.AmmoType, PlayerEnum.PlayerOne);
					}
				}
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerOne); //buJiBaoScript
				}
			}
		}
		else {
			ammoForward = obj.transform.forward;
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ammoSpawnPos, ammoForward, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				if (ammoScript.AmmoType == PlayerAmmoType.PuTongAmmo) {
					XKNpcHealthCtrl healthScript = hit.collider.GetComponent<XKNpcHealthCtrl>();
					if (healthScript != null && !healthScript.GetIsDeathNpc()) {
						healthScript.OnDamageNpc(ammoScript.AmmoType, PlayerEnum.PlayerOne);
					}
				}
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerOne); //buJiBaoScript
				}
			}
		}

		if (hit.collider == null) {
			ammoScript.StartMoveAmmo(firePos, PlayerEnum.PlayerOne);
		}
		else {
			ammoScript.StartMoveAmmo(firePos, PlayerEnum.PlayerOne, null, hit.collider.gameObject);
		}

		if (isSpawnGaoBaoDan) {
			//Call OtherPort Show AmmoParticle
			PlayerScript.CallOtherPortSpawnPlayerAmmoParticle(1, 2, firePos);
		}
		else {
			//Call OtherPort Show AmmoParticle
			PlayerScript.CallOtherPortSpawnPlayerAmmoParticle(1, 0, firePos);
		}
	}

	void CheckPlayerTwoFireBt()
	{
		if (XKPlayerCamera.IndexPlayerNum != 1) {
			SetQianGuanTwRot(PlayerEnum.PlayerTwo, false);
			return;
		}

		if (!ScreenDanHeiCtrl.IsStartGame) {
			SetQianGuanTwRot(PlayerEnum.PlayerTwo, false);
			return;
		}
		
		if (!XkGameCtrl.IsActivePlayerTwo) {
			SetQianGuanTwRot(PlayerEnum.PlayerTwo, false);
			return;
		}

		if (!IsActiveFireBtTwo) {
			SetQianGuanTwRot(PlayerEnum.PlayerTwo, false);
			return;
		}
		SetQianGuanTwRot(PlayerEnum.PlayerTwo, true);
		
//		if (DaoJiShiCtrl.GetInstance().GetIsPlayDaoJishi()) {
//			return;
//		}
		
		if (!XkGameCtrl.IsTiaoGuoStartCartoon) {
			if (!ZhunXingCtrl.GetInstanceTwo().GetActiveZhunXing() || !ZhunXingTeXiaoCtrl.IsOverTeXiaoZhunXing) {
				return;
			}
		}
		
		if (Camera.main == null) {
			return;
		}
		
//		if (GameOverCtrl.IsShowGameOver || JiFenJieMianCtrl.GetInstance().GetIsShowFinishTask()) {
//			IsActiveFireBtTwo = false;
//			return;
//		}

//		if (PlayerStEnum == PlayerTypeEnum.FeiJi && XkGameCtrl.GameJiTaiSt != GameJiTaiType.FeiJiJiTai) {
//			IsActiveFireBtTwo = false;
//			return;
//		}
		
//		if (PlayerStEnum == PlayerTypeEnum.TanKe && XkGameCtrl.GameJiTaiSt != GameJiTaiType.TanKeJiTai) {
//			IsActiveFireBtTwo = false;
//			return;
//		}

		if (XkGameCtrl.GaoBaoDanNumPTwo <= 0
		    && Time.time < LastFireTimeTwo + 1f / Frequency) {
			return;
		}

		if (XkGameCtrl.GaoBaoDanNumPTwo > 0
		    && Time.time < LastFireTimeTwo + 1f / FrequencyGaoBao) {
			return;
		}
		LastFireTimeTwo = Time.time;
		Vector3 ammoSpawnPos = AmmoStartPosTwo[0].position;
		GameObject obj = null;
		CheckFireAudioPlayerTwo();

		bool isSpawnGaoBaoDan = false;
		if (XkGameCtrl.GaoBaoDanNumPTwo <= 0) {
			if (AmmoParticle[0] != null && AmmoParticleObj[1] == null) {
				obj = (GameObject)Instantiate(AmmoParticle[0], ammoSpawnPos, AmmoStartPosTwo[0].rotation);
				AmmoParticleObj[1] = obj;
				obj.transform.parent = AmmoStartPosTwo[0];
				XkGameCtrl.CheckObjDestroyThisTimed(obj);
			}
			obj = GetPlayerAmmo(PlayerAmmoType.PuTongAmmo, ammoSpawnPos, AmmoStartPosTwo[0].rotation);
		}
		else {
			isSpawnGaoBaoDan = true;
			if (GaoBaoAmmoParticle[0] != null) {
				obj = (GameObject)Instantiate(GaoBaoAmmoParticle[0], ammoSpawnPos, AmmoStartPosTwo[0].rotation);
				obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
				XkGameCtrl.CheckObjDestroyThisTimed(obj);
			}
			XkGameCtrl.GetInstance().SubGaoBaoDanNumPTwo();
			obj = GetPlayerAmmo(PlayerAmmoType.GaoBaoAmmo, ammoSpawnPos, AmmoStartPosTwo[0].rotation);
		}
		obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
		PlayerAmmoCtrl ammoScript = obj.GetComponent<PlayerAmmoCtrl>();
		ammoScript.SetIsDonotHurtNpc(false);
		
		Vector3 mousePosInput = Input.mousePosition;
		if (pcvr.bIsHardWare) {
			mousePosInput = pcvr.CrossPositionTwo;
		}
		
		Vector3 firePos = Vector3.zero;
		Vector3 mousePos = mousePosInput + Vector3.forward * OffsetForward;
		Vector3 posTmp = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 ammoForward = Vector3.Normalize( posTmp - ammoSpawnPos );
		Ray ray = Camera.main.ScreenPointToRay(mousePosInput);
		RaycastHit hit;
		if (!IsPSAutoFire) {
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ray, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				if (ammoScript.AmmoType == PlayerAmmoType.PuTongAmmo) {
					XKNpcHealthCtrl healthScript = hit.collider.GetComponent<XKNpcHealthCtrl>();
					if (healthScript != null && !healthScript.GetIsDeathNpc()) {
						healthScript.OnDamageNpc(ammoScript.AmmoType, PlayerEnum.PlayerTwo);
					}
				}
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerTwo); //buJiBaoScript
				}
			}
		}
		else {
			ammoForward = obj.transform.forward;
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ammoSpawnPos, ammoForward, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				if (ammoScript.AmmoType == PlayerAmmoType.PuTongAmmo) {
					XKNpcHealthCtrl healthScript = hit.collider.GetComponent<XKNpcHealthCtrl>();
					if (healthScript != null && !healthScript.GetIsDeathNpc()) {
						healthScript.OnDamageNpc(ammoScript.AmmoType, PlayerEnum.PlayerTwo);
					}
				}
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerTwo); //buJiBaoScript
				}
			}
		}

		if (hit.collider == null) {
			ammoScript.StartMoveAmmo(firePos, PlayerEnum.PlayerTwo);
		}
		else {
			ammoScript.StartMoveAmmo(firePos, PlayerEnum.PlayerTwo, null, hit.collider.gameObject);
		}

		if (isSpawnGaoBaoDan) {
			//Call OtherPort Show AmmoParticle
			PlayerScript.CallOtherPortSpawnPlayerAmmoParticle(2, 2, firePos);
		}
		else {
			//Call OtherPort Show AmmoParticle
			PlayerScript.CallOtherPortSpawnPlayerAmmoParticle(2, 0, firePos);
		}
	}
	
	/// <summary>
	/// particleType == 0 ---> 普通子弹.
	/// particleType == 1 ---> 导弹.
	/// particleType == 2 ---> 高爆弹.
	/// </summary>
	public void SpawnPlayerAmmoParticle(int playerIndex, int particleType, Vector3 firePos)
	{
		if (playerIndex > 2 || playerIndex < 1) {
			playerIndex = 1;
		}
		
		if (particleType > 2 || particleType < 0) {
			particleType = 0;
		}

		int ammoSpawnIndex = 0; //real ammo spawnPoint.
		GameObject obj = null;
		switch (playerIndex) {
		case 1:
			if (particleType == 0) {
//				if (PlayerAudio[0].isPlaying) {
//					PlayerAudio[0].Stop();
//				}
//				PlayerAudio[0].Play();

				if (AmmoParticle[ammoSpawnIndex] != null) {
					obj = (GameObject)Instantiate(AmmoParticle[ammoSpawnIndex], AmmoStartPosOne[ammoSpawnIndex].position,
					                              AmmoStartPosOne[ammoSpawnIndex].rotation);
					obj.transform.parent = AmmoStartPosOne[ammoSpawnIndex];
				}

				OtherPortSpawnPlayerAmmo(PlayerAmmoType.PuTongAmmo,
				                         AmmoStartPosOne[ammoSpawnIndex].position,
				                         AmmoStartPosOne[ammoSpawnIndex].rotation,
				                         firePos);
			}
			else if (particleType == 2) {
//				if (PlayerAudio[1].isPlaying) {
//					PlayerAudio[1].Stop();
//				}
//				PlayerAudio[1].Play();

				if (GaoBaoAmmoParticle[ammoSpawnIndex] != null) {
					obj = (GameObject)Instantiate(GaoBaoAmmoParticle[ammoSpawnIndex], AmmoStartPosOne[ammoSpawnIndex].position,
					                              AmmoStartPosOne[ammoSpawnIndex].rotation);
					obj.transform.parent = AmmoStartPosOne[ammoSpawnIndex];
				}
				
				OtherPortSpawnPlayerAmmo(PlayerAmmoType.GaoBaoAmmo,
				                         AmmoStartPosOne[ammoSpawnIndex].position,
				                         AmmoStartPosOne[ammoSpawnIndex].rotation,
				                         firePos);
			}
			else if (particleType == 1) {
				if (PlayerAudio[2].isPlaying) {
					PlayerAudio[2].Stop();
				}
				PlayerAudio[2].Play();

				if (DaoDanAmmoParticle[ammoSpawnIndex] != null) {
					obj = (GameObject)Instantiate(DaoDanAmmoParticle[ammoSpawnIndex], DaoDanAmmoPosOne[ammoSpawnIndex].position,
					                              DaoDanAmmoPosOne[ammoSpawnIndex].rotation);
					obj.transform.parent = DaoDanAmmoPosOne[ammoSpawnIndex];
				}
				
				OtherPortSpawnPlayerAmmo(PlayerAmmoType.DaoDanAmmo,
				                         DaoDanAmmoPosOne[ammoSpawnIndex].position,
				                         DaoDanAmmoPosOne[ammoSpawnIndex].rotation,
				                         firePos);
			}
			break;
			
		case 2:
			if (particleType == 0) {
//				if (PlayerAudio[3].isPlaying) {
//					PlayerAudio[3].Stop();
//				}
//				PlayerAudio[3].Play();

				if (AmmoParticle[ammoSpawnIndex] != null) {
					obj = (GameObject)Instantiate(AmmoParticle[ammoSpawnIndex], AmmoStartPosTwo[ammoSpawnIndex].position,
					                              AmmoStartPosTwo[ammoSpawnIndex].rotation);
					obj.transform.parent = AmmoStartPosTwo[ammoSpawnIndex];
				}
				
				OtherPortSpawnPlayerAmmo(PlayerAmmoType.PuTongAmmo,
				                         AmmoStartPosTwo[ammoSpawnIndex].position,
				                         AmmoStartPosTwo[ammoSpawnIndex].rotation,
				                         firePos);
			}
			else if (particleType == 2) {
//				if (PlayerAudio[4].isPlaying) {
//					PlayerAudio[4].Stop();
//				}
//				PlayerAudio[4].Play();

				if (GaoBaoAmmoParticle[ammoSpawnIndex] != null) {
					obj = (GameObject)Instantiate(GaoBaoAmmoParticle[ammoSpawnIndex], AmmoStartPosTwo[ammoSpawnIndex].position,
					                              AmmoStartPosTwo[ammoSpawnIndex].rotation);
					obj.transform.parent = AmmoStartPosTwo[ammoSpawnIndex];
				}
				
				OtherPortSpawnPlayerAmmo(PlayerAmmoType.GaoBaoAmmo,
				                         AmmoStartPosTwo[ammoSpawnIndex].position,
				                         AmmoStartPosTwo[ammoSpawnIndex].rotation,
				                         firePos);
			}
			else if (particleType == 1) {
				if (PlayerAudio[5].isPlaying) {
					PlayerAudio[5].Stop();
				}
				PlayerAudio[5].Play();

				if (DaoDanAmmoParticle[ammoSpawnIndex] != null) {
					obj = (GameObject)Instantiate(DaoDanAmmoParticle[ammoSpawnIndex], DaoDanAmmoPosTwo[ammoSpawnIndex].position,
					                              DaoDanAmmoPosTwo[ammoSpawnIndex].rotation);
					obj.transform.parent = DaoDanAmmoPosTwo[ammoSpawnIndex];
				}
				
				OtherPortSpawnPlayerAmmo(PlayerAmmoType.DaoDanAmmo,
				                         DaoDanAmmoPosTwo[ammoSpawnIndex].position,
				                         DaoDanAmmoPosTwo[ammoSpawnIndex].rotation,
				                         firePos);
			}
			break;
		}

		if (obj != null) {
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localEulerAngles = Vector3.zero;
			XkGameCtrl.CheckObjDestroyThisTimed(obj);
		}
	}

	public static int PlayerAmmoNumTest;
	GameObject GetPlayerAmmo(PlayerAmmoType ammoType, Vector3 ammoPos, Quaternion ammoRot)
	{
		int max = 0;
		GameObject objAmmo = null;
		switch (ammoType) {
		case PlayerAmmoType.PuTongAmmo:
			if (PlayerStEnum == PlayerTypeEnum.FeiJi) {
				max = AmmoList.Count;
				for (int i = 0; i < max; i++) {
					if (!AmmoList[i].gameObject.activeSelf) {
						objAmmo = AmmoList[i].gameObject;
						break;
					}
				}
			}
			else if (PlayerStEnum == PlayerTypeEnum.TanKe) {
				max = AmmoList_TK.Count;
				for (int i = 0; i < max; i++) {
					if (!AmmoList_TK[i].gameObject.activeSelf) {
						objAmmo = AmmoList_TK[i].gameObject;
						break;
					}
				}
			}

			if (objAmmo == null) {
				objAmmo = SpawnPlayerAmmo(PuTongAmmo, ammoPos, ammoRot);
				HandleAmmoList( objAmmo.GetComponent<PlayerAmmoCtrl>() );
			}
			break;

		case PlayerAmmoType.GaoBaoAmmo:
			if (PlayerStEnum == PlayerTypeEnum.FeiJi) {
				max = AmmoGaoBaoList.Count;
				for (int i = 0; i < max; i++) {
					if (!AmmoGaoBaoList[i].gameObject.activeSelf) {
						objAmmo = AmmoGaoBaoList[i].gameObject;
						break;
					}
				}
			}
			else if (PlayerStEnum == PlayerTypeEnum.TanKe) {
				max = AmmoGaoBaoList_TK.Count;
				for (int i = 0; i < max; i++) {
					if (!AmmoGaoBaoList_TK[i].gameObject.activeSelf) {
						objAmmo = AmmoGaoBaoList_TK[i].gameObject;
						break;
					}
				}
			}

			if (objAmmo == null) {
				objAmmo = SpawnPlayerAmmo(GaoBaoDanAmmo, ammoPos, ammoRot);
				HandleAmmoList( objAmmo.GetComponent<PlayerAmmoCtrl>() );
			}
			break;
		}

		if (objAmmo != null) {
			Transform tranAmmo = objAmmo.transform;
			tranAmmo.position = ammoPos;
			tranAmmo.rotation = ammoRot;
		}
		return objAmmo;
	}

	void HandleAmmoList(PlayerAmmoCtrl scriptAmmo)
	{
		PlayerAmmoType ammoType = scriptAmmo.AmmoType;
		switch (ammoType) {
		case PlayerAmmoType.PuTongAmmo:
			if (PlayerStEnum == PlayerTypeEnum.FeiJi) {
				if (AmmoList.Contains(scriptAmmo)) {
					return;
				}
				AmmoList.Add(scriptAmmo);
			}
			else if (PlayerStEnum == PlayerTypeEnum.TanKe) {
				if (AmmoList_TK.Contains(scriptAmmo)) {
					return;
				}
				AmmoList_TK.Add(scriptAmmo);
			}
			break;

		case PlayerAmmoType.GaoBaoAmmo:
			if (PlayerStEnum == PlayerTypeEnum.FeiJi) {
				if (AmmoGaoBaoList.Contains(scriptAmmo)) {
					return;
				}
				AmmoGaoBaoList.Add(scriptAmmo);
			}
			else if (PlayerStEnum == PlayerTypeEnum.TanKe) {
				if (AmmoGaoBaoList_TK.Contains(scriptAmmo)) {
					return;
				}
				AmmoGaoBaoList_TK.Add(scriptAmmo);
			}
			break;
		}
	}

	void ClickFireBtOneEvent(ButtonState state)
	{
		//Debug.Log("ClickFireBtOneEvent***state "+state);
		if (state == ButtonState.DOWN) {
			IsActiveFireBtOne = true;
		}
		else {
			IsActiveFireBtOne = false;
		}
	}

	void ClickFireBtTwoEvent(ButtonState state)
	{
		//Debug.Log("ClickFireBtOneEvent***state "+state);
		if (state == ButtonState.DOWN) {
			IsActiveFireBtTwo = true;
		}
		else {
			IsActiveFireBtTwo = false;
		}
	}

	void ClickDaoDanBtOneEvent(ButtonState state)
	{
		if (XKPlayerCamera.IndexPlayerNum != 0) {
			return;
		}

		if (XKTriggerClosePlayerUI.IsClosePlayerUI) {
			return;
		}

		if (!gameObject.activeSelf) {
			return;
		}

		if (!ScreenDanHeiCtrl.IsStartGame) {
			return;
		}

		if (!XkGameCtrl.IsActivePlayerOne) {
			return;
		}

		if (state != ButtonState.DOWN) {
			return;
		}

		if (XkGameCtrl.GetInstance().GetDaoDanNumPOne() <= 0) {
			XKGlobalData.GetInstance().PlayDaoDanJingGaoAudio();
			return;
		}
		
//		if (PlayerStEnum == PlayerTypeEnum.FeiJi && XkGameCtrl.GameJiTaiSt != GameJiTaiType.FeiJiJiTai) {
//			return;
//		}
//		
//		if (PlayerStEnum == PlayerTypeEnum.TanKe && XkGameCtrl.GameJiTaiSt != GameJiTaiType.TanKeJiTai) {
//			return;
//		}

		if (Camera.main == null) {
			return;
		}
		
//		if (!ZhunXingCtrl.GetInstanceOne().GetActiveZhunXing() || !ZhunXingTeXiaoCtrl.IsOverTeXiaoZhunXing) {
//			return;
//		}

		if (Time.realtimeSinceStartup - DaoDanTimeVal[0] < DaoDanTimeMin) {
			XKGlobalData.GetInstance().PlayDaoDanJingGaoAudio();
			return;
		}
		DaoDanTimeVal[0] = Time.realtimeSinceStartup;

		Vector3 ammoSpawnPos = DaoDanAmmoPosOne[0].position;
		GameObject obj = null;
		if (DaoDanAmmoParticle[0] != null) {
			obj = (GameObject)Instantiate(DaoDanAmmoParticle[0], ammoSpawnPos, DaoDanAmmoPosOne[0].rotation);
			obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
			XkGameCtrl.CheckObjDestroyThisTimed(obj);
		}
		
		if (PlayerAudio[2].isPlaying) {
			PlayerAudio[2].Stop();
		}
		PlayerAudio[2].Play();

		obj = SpawnPlayerAmmo(DaoDanAmmo, ammoSpawnPos, DaoDanAmmoPosOne[0].rotation);
		obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
		PlayerAmmoCtrl ammoScript = obj.GetComponent<PlayerAmmoCtrl>();
		XkGameCtrl.GetInstance().SubDaoDanNumPOne();
		
		Vector3 mousePosInput = Input.mousePosition;
		if (pcvr.bIsHardWare) {
			mousePosInput = pcvr.CrossPositionOne;
		}
		
		Vector3 firePos = Vector3.zero;
		Vector3 mousePos = mousePosInput + Vector3.forward * OffsetForward;
		Vector3 posTmp = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 ammoForward = Vector3.Normalize( posTmp - ammoSpawnPos );
		firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
		Ray ray = Camera.main.ScreenPointToRay(mousePosInput);
		RaycastHit hit;
		if (!IsPSAutoFire) {
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ray, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerOne); //buJiBaoScript
				}
			}
		}
		else {
			ammoForward = obj.transform.forward;
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ammoSpawnPos, ammoForward, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerOne); //buJiBaoScript
				}
			}
		}
		ammoScript.StartMoveAmmo(firePos, PlayerEnum.PlayerOne);

		//Call OtherPort Show DaoDanAmmoParticle
		PlayerScript.CallOtherPortSpawnPlayerAmmoParticle(1, 1, firePos);
	}

	void ClickDaoDanBtTwoEvent(ButtonState state)
	{
		if (XKPlayerCamera.IndexPlayerNum != 1) {
			return;
		}

		if (XKTriggerClosePlayerUI.IsClosePlayerUI) {
			return;
		}

		if (!gameObject.activeSelf) {
			return;
		}

		if (!ScreenDanHeiCtrl.IsStartGame) {
			return;
		}

		if (!XkGameCtrl.IsActivePlayerTwo) {
			return;
		}

		if (state != ButtonState.DOWN) {
			return;
		}

		if (XkGameCtrl.GetInstance().GetDaoDanNumPTwo() <= 0) {
			XKGlobalData.GetInstance().PlayDaoDanJingGaoAudio();
			return;
		}

//		if (PlayerStEnum == PlayerTypeEnum.FeiJi && XkGameCtrl.GameJiTaiSt != GameJiTaiType.FeiJiJiTai) {
//			return;
//		}
//		
//		if (PlayerStEnum == PlayerTypeEnum.TanKe && XkGameCtrl.GameJiTaiSt != GameJiTaiType.TanKeJiTai) {
//			return;
//		}
		
		if (Camera.main == null) {
			return;
		}
		
//		if (!ZhunXingCtrl.GetInstanceTwo().GetActiveZhunXing() || !ZhunXingTeXiaoCtrl.IsOverTeXiaoZhunXing) {
//			return;
//		}
		
		if (Time.realtimeSinceStartup - DaoDanTimeVal[1] < DaoDanTimeMin) {
			XKGlobalData.GetInstance().PlayDaoDanJingGaoAudio();
			return;
		}
		DaoDanTimeVal[1] = Time.realtimeSinceStartup;

		Vector3 ammoSpawnPos = DaoDanAmmoPosTwo[0].position;
		GameObject obj = null;
		if (DaoDanAmmoParticle[0] != null) {
			obj = (GameObject)Instantiate(DaoDanAmmoParticle[0], ammoSpawnPos, DaoDanAmmoPosTwo[0].rotation);
			obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
			XkGameCtrl.CheckObjDestroyThisTimed(obj);
		}
		
		if (PlayerAudio[5].isPlaying) {
			PlayerAudio[5].Stop();
		}
		PlayerAudio[5].Play();

		obj = SpawnPlayerAmmo(DaoDanAmmo, ammoSpawnPos, DaoDanAmmoPosTwo[0].rotation);
		obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
		PlayerAmmoCtrl ammoScript = obj.GetComponent<PlayerAmmoCtrl>();
		XkGameCtrl.GetInstance().SubDaoDanNumPTwo();
		
		Vector3 mousePosInput = Input.mousePosition;
		if (pcvr.bIsHardWare) {
			mousePosInput = pcvr.CrossPositionTwo;
		}
		
		Vector3 firePos = Vector3.zero;
		Vector3 mousePos = mousePosInput + Vector3.forward * OffsetForward;
		Vector3 posTmp = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 ammoForward = Vector3.Normalize( posTmp - ammoSpawnPos );
		Ray ray = Camera.main.ScreenPointToRay(mousePosInput);
		RaycastHit hit;
		if (!IsPSAutoFire) {
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ray, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerTwo); //buJiBaoScript
				}
			}
		}
		else {
			ammoForward = obj.transform.forward;
			firePos = FirePosValTmp * ammoForward + ammoSpawnPos;
			if (Physics.Raycast(ammoSpawnPos, ammoForward, out hit, FireRayDirLen, FireLayer.value)) {
				//Debug.Log("Player fire obj -> "+hit.collider.name);
				firePos = hit.point;
				
				BuJiBaoCtrl buJiBaoScript = hit.collider.GetComponent<BuJiBaoCtrl>();
				if (buJiBaoScript != null) {
					buJiBaoScript.RemoveBuJiBao(PlayerEnum.PlayerTwo); //buJiBaoScript
				}
			}
		}
		ammoScript.StartMoveAmmo(firePos, PlayerEnum.PlayerTwo);

		//Call OtherPort Show DaoDanAmmoParticle
		PlayerScript.CallOtherPortSpawnPlayerAmmoParticle(2, 1, firePos);
	}

	public void SpawnPlayerDaoDan(Transform ammoTran, GameObject playerDaoDan)
	{
		//Debug.Log("SpawnPlayerDaoDan***");
		Vector3 ammoSpawnPos = ammoTran.position;
		GameObject obj = SpawnPlayerAmmo(playerDaoDan, ammoSpawnPos, ammoTran.rotation);
		obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
		PlayerAmmoCtrl ammoScript = obj.GetComponent<PlayerAmmoCtrl>();
		RaycastHit hitInfo;
		float disVal = Random.Range(300f, 500f);
		Vector3 forwardVal = ammoTran.forward;
		Vector3 firePos = ammoSpawnPos + (forwardVal * disVal);
		Physics.Raycast(ammoSpawnPos, forwardVal, out hitInfo, disVal, FireLayer.value);
		if (hitInfo.collider != null){
			firePos = hitInfo.point;
		}
		ammoScript.StartMoveAmmo(firePos, PlayerEnum.Null);
	}

	void CheckFireAudioPlayerOne()
	{
		if (!IsActiveFireBtOne) {
			return;
		}

		if (IsActiveFireBtTwo && XkGameCtrl.IsActivePlayerTwo) {
			PlayerAudio[0].volume = SRPTFireVolume;
			PlayerAudio[1].volume = SRGBFireVolume;
		}
		else {
			PlayerAudio[0].volume = DRPTFireVolume;
			PlayerAudio[1].volume = DRGBFireVolume;
		}

		if (XkGameCtrl.GaoBaoDanNumPOne <= 0) {
			if (PlayerAudio[0].isPlaying) {
				PlayerAudio[0].Stop();
			}
			PlayerAudio[0].Play();
		}
		else {
			if (PlayerAudio[1].isPlaying) {
				PlayerAudio[1].Stop();
			}
			PlayerAudio[1].Play();
		}
	}
	
	void CheckFireAudioPlayerTwo()
	{
		if (!IsActiveFireBtTwo) {
			return;
		}
		
		if (IsActiveFireBtOne && XkGameCtrl.IsActivePlayerOne) {
			PlayerAudio[3].volume = SRPTFireVolume;
			PlayerAudio[4].volume = SRGBFireVolume;
		}
		else {
			PlayerAudio[3].volume = DRPTFireVolume;
			PlayerAudio[4].volume = DRGBFireVolume;
		}

		if (XkGameCtrl.GaoBaoDanNumPTwo <= 0) {
			if (PlayerAudio[3].isPlaying) {
				PlayerAudio[3].Stop();
			}
			PlayerAudio[3].Play();
		}
		else {
			if (PlayerAudio[4].isPlaying) {
				PlayerAudio[4].Stop();
			}
			PlayerAudio[4].Play();
		}
	}

	void CheckPSTriggerAutoFire()
	{
		if (!XkGameCtrl.GetInstance().IsCartoonShootTest) {
			return;
		}

		PlayerAmmoType ammoType = PSTriggerCamera.AutoFirePlayerAmmoTypeVal;
		if (PSAmmoTypeVal == ammoType) {
			switch (PSAmmoTypeVal) {
			case PlayerAmmoType.PuTongAmmo:
			case PlayerAmmoType.GaoBaoAmmo:
				if (!IsActiveFireBtOne) {
					IsActiveFireBtOne = true;
				}

				if (!IsActiveFireBtTwo) {
					IsActiveFireBtTwo = true;
				}

				if (PSAmmoTypeVal == PlayerAmmoType.GaoBaoAmmo) {
					XkGameCtrl.GaoBaoDanNumPOne = 9999;
					XkGameCtrl.GaoBaoDanNumPTwo = 9999;
				}
				else {
					XkGameCtrl.GaoBaoDanNumPOne = 0;
					XkGameCtrl.GaoBaoDanNumPTwo = 0;
				}
				break;

			case PlayerAmmoType.DaoDanAmmo:
				ClickDaoDanBtOneEvent(ButtonState.DOWN);
				ClickDaoDanBtTwoEvent(ButtonState.DOWN);
				break;

			default:
				if (IsActiveFireBtOne) {
					IsActiveFireBtOne = false;
				}
				
				if (IsActiveFireBtTwo) {
					IsActiveFireBtTwo = false;
				}
				break;
			}
			return;
		}
		PSAmmoTypeVal = ammoType;

		if (PSAmmoTypeVal == PlayerAmmoType.DaoDanAmmo
		    || PSAmmoTypeVal == PlayerAmmoType.GaoBaoAmmo
		    || PSAmmoTypeVal == PlayerAmmoType.PuTongAmmo) {
			IsPSAutoFire = true;
		}
		else {
			IsPSAutoFire = false;
		}
	}

	void CheckCrossAimObjPlayerOne()
	{
		if (!XkGameCtrl.IsActivePlayerOne) {
			return;
		}

		if (Camera.main == null) {
			return;
		}

		if (XkGameCtrl.GameModeVal != GameMode.LianJi) {
			return;
		}

		if (XkGameCtrl.GameJiTaiSt == GameJiTaiType.FeiJiJiTai && PlayerStEnum != PlayerTypeEnum.FeiJi) {
			return;
		}

		if (XkGameCtrl.GameJiTaiSt == GameJiTaiType.TanKeJiTai && PlayerStEnum != PlayerTypeEnum.TanKe) {
			return;
		}

		if (Network.peerType == NetworkPeerType.Server) {
			return;
		}

		Vector3 mousePosInput = Input.mousePosition;
		if (pcvr.bIsHardWare) {
			mousePosInput = pcvr.CrossPositionOne;
		}
		
		Ray ray = Camera.main.ScreenPointToRay(mousePosInput);
		RaycastHit hit;
		bool  isAimPlayer = false;
		if (Physics.Raycast(ray, out hit, FireRayDirLen, XkGameCtrl.GetInstance().PlayerLayer.value)) {
			if (hit.collider.tag == "Player") {
				if (PlayerStEnum == PlayerTypeEnum.TanKe) {
					PlayerZhiShengJiCtrl zsjScript = hit.collider.GetComponent<PlayerZhiShengJiCtrl>();
					if (zsjScript == null) {
						isAimPlayer = true;
					}
				}
				else {
					isAimPlayer = true;
				}
				//Debug.Log("********************aim player p1");
			}
		}

		if (IsAimPlayerPOne && !isAimPlayer && Time.realtimeSinceStartup - TimeAimPlayerPOne < 0.5f) {
			return;
		}
		TimeAimPlayerPOne = Time.realtimeSinceStartup;

		if (IsAimPlayerPOne != isAimPlayer) {
			IsAimPlayerPOne = isAimPlayer;
			ZhunXingCtrl.GetInstanceOne().SetZhunXingSprite();
		}
	}

	void CheckCrossAimObjPlayerTwo()
	{
		if (!XkGameCtrl.IsActivePlayerTwo) {
			return;
		}
		
		if (Camera.main == null) {
			return;
		}
		
		if (XkGameCtrl.GameModeVal != GameMode.LianJi) {
			return;
		}
		
		if (XkGameCtrl.GameJiTaiSt == GameJiTaiType.FeiJiJiTai && PlayerStEnum != PlayerTypeEnum.FeiJi) {
			return;
		}
		
		if (XkGameCtrl.GameJiTaiSt == GameJiTaiType.TanKeJiTai && PlayerStEnum != PlayerTypeEnum.TanKe) {
			return;
		}
		
		if (Network.peerType == NetworkPeerType.Server) {
			return;
		}
		
		Vector3 mousePosInput = Input.mousePosition;
		if (pcvr.bIsHardWare) {
			mousePosInput = pcvr.CrossPositionOne;
		}

		Ray ray = Camera.main.ScreenPointToRay(mousePosInput);
		RaycastHit hit;
		bool  isAimPlayer = false;
		if (Physics.Raycast(ray, out hit, FireRayDirLen, XkGameCtrl.GetInstance().PlayerLayer.value)) {
			if (hit.collider.tag == "Player") {
				if (PlayerStEnum == PlayerTypeEnum.TanKe) {
					PlayerZhiShengJiCtrl zsjScript = hit.collider.GetComponent<PlayerZhiShengJiCtrl>();
					if (zsjScript == null) {
						isAimPlayer = true;
					}
				}
				else {
					isAimPlayer = true;
				}
				//Debug.Log("********************aim player p2");
			}
		}
		
		if (IsAimPlayerPTwo && !isAimPlayer && Time.realtimeSinceStartup - TimeAimPlayerPTwo < 0.5f) {
			return;
		}
		TimeAimPlayerPTwo = Time.realtimeSinceStartup;

		if (IsAimPlayerPTwo != isAimPlayer) {
			IsAimPlayerPTwo = isAimPlayer;
			ZhunXingCtrl.GetInstanceTwo().SetZhunXingSprite();
		}
	}
	
	void OtherPortSpawnPlayerAmmo(PlayerAmmoType ammoType, Vector3 ammoSpawnPos, Quaternion ammoSpawnRot, Vector3 firePos)
	{
		//GameObject obj = SpawnPlayerAmmo(ammoPlayer, ammoSpawnPos, ammoSpawnRot);
		GameObject obj = null;
		switch (ammoType) {
		case PlayerAmmoType.PuTongAmmo:
		case PlayerAmmoType.GaoBaoAmmo:
			obj = GetPlayerAmmo(ammoType, ammoSpawnPos, ammoSpawnRot);
			break;

		default:
			obj = SpawnPlayerAmmo(DaoDanAmmo, ammoSpawnPos, ammoSpawnRot);
			break;
		}
		obj.transform.parent = XkGameCtrl.PlayerAmmoArray;
		PlayerAmmoCtrl ammoScript = obj.GetComponent<PlayerAmmoCtrl>();
		ammoScript.SetIsDonotHurtNpc(true);
		ammoScript.StartMoveAmmo(firePos, PlayerEnum.Null);
	}

	/*public List<AmmoParticleDt> AmmoParticleList;
	GameObject SpawnPlayerFireParticle(GameObject ammoParticle, Vector3 pos, Quaternion rot)
	{
		if (ammoParticle != null) {
			return null;
		}

		GameObject obj = null;
		Transform tran = null;
		XKFireParticleCtrl fireParticleScript = null;
		int max = AmmoParticleList.Count;
		for (int i = 0; i < max; i++) {
			if (AmmoParticleList[i] != null
			    && !AmmoParticleList[i].ParticleObj.activeSelf
			    && AmmoParticleList[i].ParticleObj.name == ammoParticle.name) {
				//obj = AmmoParticleList[i];
				tran = obj.transform;
				tran.position = pos;
				tran.rotation = rot;
				fireParticleScript = obj.GetComponent<XKFireParticleCtrl>();
				if (fireParticleScript != null) {
					fireParticleScript.OpenGunParticle();
				}
				break;
			}
		}

		if (obj == null) {
			obj = (GameObject)Instantiate(ammoParticle, pos, rot);
			obj.name = ammoParticle.name;
//			if (!AmmoParticleList.Contains(obj)) {
//				AmmoParticleList.Add(obj);
//			}
		}
		return obj;
	}*/
}