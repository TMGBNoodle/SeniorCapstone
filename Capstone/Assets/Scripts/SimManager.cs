using UnityEngine;

public class SimManager : MonoBehaviour
{
    float timeBetweenSwitch = 20;

    private float timer;
    public GameObject interpreter;
    public GameObject creatureDrawer;
    float lastSwitch;

    float currentTime;

    GameObject currentCreature;
    Vector3 startPos = new Vector3(-100, 0, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 100;
        currentTime = Time.time;
        lastSwitch = Time.time;
        
        interpreter.GetComponent<DNAInterp>().input.text = RandomString();
        currentCreature = creatureDrawer.GetComponent<DrawCreature>().sendPart();
    }

    // Update is called once per frame
    void Update()
    {
        // timer += Time.deltaTime;
        // while (timer >= Time.fixedDeltaTime)
        // {
        //     timer -= Time.fixedDeltaTime;
        //     Physics.Simulate(0.003f);
        // }
        currentTime = Time.time;
        print(currentTime);
        print(lastSwitch);
        if(currentTime - lastSwitch > timeBetweenSwitch)
        {
            print("StopTraining");
            currentCreature.GetComponent<CreatureControl>().tr2 = false;
            if(currentCreature.GetComponent<CreatureControl>().saved == true)
            {
                lastSwitch = Time.time;
                print("Mutate");
                interpreter.GetComponent<DNAInterp>().mutate();
                currentCreature = creatureDrawer.GetComponent<DrawCreature>().sendPart();
            }
        }
    }

    public static string RandomString(int length = 15)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] buffer = new char[length];

        for (int i = 0; i < length; i++)
            buffer[i] = chars[Random.Range(0,(chars.Length))];

        return new string(buffer);
    }

    // void trainSim(GameObject creature)
    // {
    //     creature.transform.position = startPos;
    //     creature.SetActive(true);
    //     creature.GetComponent<Rigidbody2D>().simulated = true;
    //     creature.GetComponent<CreatureControl>().tra
    // }
    void stopSim()
    {
        
    }
}
