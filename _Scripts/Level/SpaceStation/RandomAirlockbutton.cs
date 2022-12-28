using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomAirlockbutton : MonoBehaviour
{
    public List<AirlockOpener> Buttons;
    public static RandomAirlockbutton Instance;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var btn = Buttons[Random.Range(0, Buttons.Count)];
        Buttons.Where(a => a != btn).ToList().ForEach(a => a.gameObject.SetActive(false));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Done()
    {
        var btn = Buttons[Random.Range(0, Buttons.Count)];
        btn.gameObject.SetActive(true);
        Buttons.Where(a => a != btn).ToList().ForEach(a => a.gameObject.SetActive(false));
    }
    //void OnTriggerEnter()
    //{
    //    Buttons[Random.Range(0, Buttons.Count)];
    //}
}
