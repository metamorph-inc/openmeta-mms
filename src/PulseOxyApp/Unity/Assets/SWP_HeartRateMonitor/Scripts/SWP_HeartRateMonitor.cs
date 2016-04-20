
using UnityEngine;
using System.Collections;

/// <summary>
/// This is the Heart Rate Monitor main script and controls every element of the control.
/// </summary>
public class SWP_HeartRateMonitor : MonoBehaviour
{
	public int BeatsPerMinute = 90; // Beats per minute.
	public int OxyLevel = 20; //Oxyinagation Level
	public bool FlatLine = false; // Initialise a flat line.
	public bool ShowBlip = true; // Show the blip circle at the front of the monitor line.
	public GameObject Blip; // The blip game object.
	public float BlipSize = 1f; // The size of the blip circle at the front of the line.
	public float BlipTrailStartSize = 0.2f; // The size of the monitor trail line nearest to the blip circle.
	public float BlipTrailEndSize = 0.1f; // The size of the monitor line at the end before it fades out.
	public float BlipMonitorWidth = 40f; // The actual width of the entire monitor control.
	public float BlipMonitorHeightModifier = 1f; // The actual height of the entire monitor control.
		
	public bool EnableSound = true;
	public float HeartVolume = 1f;
	public AudioClip Heart1;
	public AudioClip Heart2;
	public AudioClip Flatline;
	private bool bFlatLinePlayed = false;
	
	private bool EnableRealWave = false; //Work In Progress
	
	private float LineSpeed = 0.3f;
	private GameObject NewClone;
	private float TrailTime;
	private float BeatsPerSecond;
	private float LastUpdate = 0f;
	private Vector3 BlipOffset = Vector3.zero;
	private float DisplayXEnd;
	public GUIText HeartRateText;
	public GUIText OxyLevelText;
	

	void Start()
	{
		BeatsPerSecond = 60f / BeatsPerMinute;
		BlipOffset = new Vector3 (transform.position.x - (BlipMonitorWidth / 2), transform.position.y, transform.position.z);
		DisplayXEnd = BlipOffset.x + BlipMonitorWidth;
		CreateClone();
		TrailTime = NewClone.GetComponentInChildren<TrailRenderer>().time;
		HeartRateText.text = BeatsPerMinute.ToString();
		OxyLevelText.text = OxyLevel.ToString()+"%";
	}
	

	void Update()
	{
		BeatsPerSecond = 60f / BeatsPerMinute;
		HeartRateText.text = BeatsPerMinute.ToString();
		OxyLevelText.text = OxyLevel.ToString()+"%";
		BlipOffset = new Vector3 (transform.position.x - (BlipMonitorWidth / 2), transform.position.y, transform.position.z);
		DisplayXEnd = BlipOffset.x + BlipMonitorWidth;
		
		if (NewClone.transform.position.x > DisplayXEnd)
		{			
			if (NewClone != null)
			{
				GameObject OldClone = NewClone;
				StartCoroutine(WaitThenDestroy(OldClone));
			}
			
			CreateClone();
		}
		else if (!FlatLine)
			NewClone.transform.position += new Vector3(BlipMonitorWidth * Time.deltaTime * LineSpeed, (!EnableRealWave ? Random.Range(-0.05f, 0.05f) : 0f), 0);
		else
		{
			NewClone.transform.position += new Vector3(BlipMonitorWidth * Time.deltaTime * LineSpeed, 0, 0);
			
			if (!bFlatLinePlayed)
			{
				PlayHeartSound(3, HeartVolume);
				bFlatLinePlayed = true;
			}
		}
		
		if (BeatsPerMinute == 0 )
			LastUpdate = Time.time;
		else if (Time.time - LastUpdate >= BeatsPerSecond)
		{			
			LastUpdate = Time.time;
			StartCoroutine(PerformBlip());
		}
	}
	
	IEnumerator PerformBlip()
	{
		if (bFlatLinePlayed)
			bFlatLinePlayed = false;
		
		if (!EnableRealWave)
		{
			if (!bFlatLinePlayed)
				PlayHeartSound(1, HeartVolume);
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (10f * BlipMonitorHeightModifier) + Random.Range(0f, (2f * BlipMonitorHeightModifier)) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.03f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (-5f * BlipMonitorHeightModifier) - Random.Range(0f, (3f * BlipMonitorHeightModifier)) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.02f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (3f * BlipMonitorHeightModifier) + Random.Range(0f, (2f * BlipMonitorHeightModifier)) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.02f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (2f * BlipMonitorHeightModifier) + Random.Range(0f, (1f * BlipMonitorHeightModifier)) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.02f);		
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, 0f + BlipOffset.y, BlipOffset.z);

			yield return new WaitForSeconds(0.2f);		
			
			if (!bFlatLinePlayed)
				PlayHeartSound(2, HeartVolume);
		}
		else
		{
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (1f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (1.5f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (2f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.03f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (1.2f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (1f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (0f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.1f);		
			  
			if (!bFlatLinePlayed)
				PlayHeartSound(1, HeartVolume);
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (-1f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (10f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.03f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (-3f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.02f);		
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (0f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.2f);		

			if (!bFlatLinePlayed)
				PlayHeartSound(2, HeartVolume);
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (1.0f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (2.3f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (2.5f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.05f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (2.3f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (2.0f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, (1f * BlipMonitorHeightModifier) + BlipOffset.y, BlipOffset.z);
			yield return new WaitForSeconds(0.01f);		
			
			NewClone.transform.position = new Vector3(NewClone.transform.position.x, 0f + BlipOffset.y, BlipOffset.z);
		}
	}
	
	void CreateClone()
	{
		NewClone = Instantiate(Blip, new Vector3(BlipOffset.x, BlipOffset.y, BlipOffset.z), Quaternion.identity) as GameObject;
		NewClone.transform.parent = gameObject.transform;
		
		NewClone.GetComponentInChildren<TrailRenderer>().startWidth = BlipTrailStartSize;
		NewClone.GetComponentInChildren<TrailRenderer>().endWidth = BlipTrailEndSize;
		NewClone.transform.localScale = new Vector3(BlipSize,BlipSize,BlipSize);
		
		if (ShowBlip)
			NewClone.GetComponent<MeshRenderer>().enabled = true;
		else
			NewClone.GetComponent<MeshRenderer>().enabled = false;
	}
	
	IEnumerator WaitThenDestroy(GameObject OldClone)
	{
		OldClone.GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds(TrailTime);
		DestroyObject(OldClone);
	}
	
	void PlayHeartSound(int iSoundType, float fSoundVolume)
	{
		if (!EnableSound)
			return;
		
		if (iSoundType == 1)
			GetComponent<AudioSource>().PlayOneShot(Heart1, fSoundVolume);
		else if (iSoundType == 2)
			GetComponent<AudioSource>().PlayOneShot(Heart2, fSoundVolume);
		else if (iSoundType == 3)
			GetComponent<AudioSource>().PlayOneShot(Flatline, fSoundVolume);
	}
}
