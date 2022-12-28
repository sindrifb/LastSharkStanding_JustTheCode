using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
	private class Square
	{
		public float minX = float.MaxValue;
		public float maxX = float.MinValue;
		public float minZ = float.MaxValue;
		public float maxZ = float.MinValue;
	}
    public bool DrawGizmos;
	[SerializeField]
	private Vector2 CameraCage = new Vector2();
	[SerializeField]
	private Vector2 Offset = new Vector2 ();
	[SerializeField]
	private float Speed = 1;
	[SerializeField]
	private float MaxTraveldistance = 1;
	[SerializeField]
	private float CameraOffset = 9;

    public float ZOffset;
    private bool zoom = false;
    private float StartMaxTravel;
    private float StartFov;
    private float StartY;

	public PlayerController[] Players;
    private PlayerController RoundWinner;
    private Vector3 LevelCenter;

    private Camera camera;

    private void Awake()
    {
        var cameras = GetComponentsInChildren<Camera>();
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].eventMask = 0;
        }
    }

    private void Start ()
	{
        StartY = transform.position.y;
        camera = GetComponent<Camera>();
        StartFov = camera.fieldOfView;
        StartMaxTravel = MaxTraveldistance;
		Players = FindObjectsOfType <PlayerController>();
        StartCoroutine(UpdatePlayers());
        EventManager.StartListening(EventManager.EventCodes.RoundEnd,UpdatePlayersInstant);
        EventManager.StartListening(EventManager.EventCodes.GameEnd, UpdatePlayersInstant);
    }

    private void UpdatePlayersInstant()
    {
        Players = FindObjectsOfType<PlayerController>().Where(a => !a.GetComponent<Death>().IsDead).ToArray();
    }

    private IEnumerator UpdatePlayers()
    {
        while (true)
        {
            Players = FindObjectsOfType<PlayerController>().Where(a => !a.GetComponent<Death>().IsDead).ToArray();

            if (Players.Length > 1 || Players.Length == 0)
            {
                zoom = false;
            }
            yield return new WaitForSeconds(.25f);
        }
    }

    public void Zoom()
    {
        StartCoroutine(ZoomCoroutine());
    }


    private void LateUpdate ()
	{
		Move();

        if (zoom)
        {
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, 30, Time.unscaledDeltaTime * 60);
        }
        else
        {
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, StartFov, Time.unscaledDeltaTime * 40);
            MaxTraveldistance = StartMaxTravel;
            Speed = 1f;
            //Time.timeScale = 1f;
        }
	}

    IEnumerator ZoomCoroutine()
    {
        UpdatePlayersInstant();
        zoom = true;
        Speed = 15f;
        MaxTraveldistance = 30f;
        RoundWinner = Players.FirstOrDefault();
        //bool isGrounded = FindObjectsOfType<PlayerController>().FirstOrDefault(a => !a.GetComponent<Death>().IsDead)?.GetComponent<CheckIfGrounded>().onGround ?? false;
        //if (!isGrounded)
        //{
        //    Time.timeScale = .05f;
        //    Time.fixedDeltaTime = 0.0005f;
        //    Speed = 40f;
        //}
        //else
        //{
        //    yield return new WaitForSeconds(1f);
        //}

        Time.timeScale = .05f;
        Time.fixedDeltaTime = 0.0005f;
        Speed = 40f;

        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.005f;
        Speed = 5f;
    }

	private void Move()
	{
		LevelCenter = new Vector3 (Offset.x, 1, Offset.y);
		Vector3 targetPosition = GetGroupCenter() - LevelCenter;

		if(targetPosition.sqrMagnitude > MaxTraveldistance * MaxTraveldistance)
		{
			targetPosition = targetPosition.normalized * MaxTraveldistance;
		}
		targetPosition += LevelCenter;

		targetPosition.y = transform.position.y;
		targetPosition.z -= CameraOffset;
        if (zoom)
        {
            if (Time.timeScale <.9f)
            {
                float targetY = 0;
                //if (Players.Any())
                //{
                //    targetY = Players.FirstOrDefault().transform.position.y;
                //}
                if (RoundWinner != null)
                {
                    targetY = RoundWinner.transform.position.y;
                }


                if (targetY < -1)
                {
                    targetY = StartY;
                }
                else
                {
                    targetPosition.y = StartY + targetY;
                }
            }
           
            targetPosition.z += ZOffset;
            
        }
        else
        {
            targetPosition.y = StartY;
        }
		transform.position = Vector3.Lerp(transform.position, targetPosition, Speed * Time.deltaTime);
	}

	public Vector3 GetGroupCenter()
	{
		Square square = GetGroupSquare();
		Vector3 center = Vector3.zero;
        int length = Players?.Length ?? 0;

        if (length > 1) 
		{
			Vector2 leftTopCorner = new Vector2(square.minX,square.maxZ);
			Vector2 rightBottomCorner = new Vector2(square.maxX, square.minZ);
			Vector2 diaCenter = (rightBottomCorner - leftTopCorner) / 2 + leftTopCorner;

			center = new Vector3(diaCenter.x , 1 ,diaCenter.y);
		}
		else if (length == 1)
		{
            center = Vector3.zero;
            if (Players[0] != null)
            {
                center = Players[0].transform.position;
            }
            center.y = 1f;
		}
        else if (length <= 0)
        {
            center = LevelCenter;
        }
		return center;
	}

	private Vector3 GetGroupRectangleSize()
	{
		Square square = GetGroupSquare ();
		Vector3 size;
        int length = Players?.Length ?? 0;

        if (length > 1) 
		{
			float sizeX =  Mathf.Abs(square.maxX - square.minX);
			float sizeZ =  Mathf.Abs(square.maxZ - square.minZ);
			size = new Vector3 (sizeX, 1 ,sizeZ);
		} 
		else 
		{
			size = new Vector3 (2,1,2);
		}

		return size;
	}

	private Square GetGroupSquare()
	{
		Square square = new Square();
        int length = Players?.Length ?? 0;
        //make rectangle out of extremes
        for (int i = 0; i < length; i++)
        { 

            Vector3 playerpos = Vector3.zero;
            if (Players[i] != null)
            {
                playerpos = Players[i]?.transform?.position ?? Vector3.zero;
            }
         
			//Vector3 playerpos = Players[i]?.transform?.position ?? Vector3.zero;

			if (playerpos.x <= square.minX)
				square.minX = playerpos.x;

			if (playerpos.x > square.maxX)
				square.maxX = playerpos.x;

			if (playerpos.z <= square.minZ)
				square.minZ = playerpos.z;

			if (playerpos.z > square.maxZ) 
				square.maxZ = playerpos.z;
		}
		return square;
	}

	private void OnDrawGizmos()
	{
        if (!DrawGizmos)
        {
            return;
        }
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(new Vector3(Offset.x, 1 , Offset.y), new Vector3(CameraCage.x, 1,CameraCage.y));

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(GetGroupCenter(),GetGroupRectangleSize());
		Gizmos.DrawWireSphere (GetGroupCenter(), 0.5f);
	}
}