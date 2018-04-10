using UnityEngine;
using System.Collections;

public class Bomb : Item
{
    private bool isTriggered;
    public void Trigger(Player activator) { isTriggered = true; triggerer = activator; }
    private Player triggerer;

    private bool coroutineRunning;

    [Space]
    [SerializeField]
    private float time;
    [SerializeField]
    private float explosionDelay;
    private float timer;

    [Space]
    [SerializeField]
    private Color flashColor;
    private Color startColor;

    [Space]
    [SerializeField]
    private Sprite bombImage;

    public override void Start()
    {
        startColor = this.GetComponent<SpriteRenderer>().color;
        startingScale = this.transform.localScale;
        GetComponent<SpriteRenderer>().sprite = bombImage;
    }

    public override void Update()
    {
        base.Update();

        //if (Input.GetKeyDown(KeyCode.KeypadPlus)) //Debugging!
        if(isTriggered && !coroutineRunning)
        {
            StartCoroutine(Bomb_Coroutine());
        }
    }

    public override void GetPickedUpByPlayer(Player player)
    {
        base.GetPickedUpByPlayer(player);

        this.Trigger(player);
    }

    public IEnumerator Bomb_Coroutine()
    {
        coroutineRunning = true;
        SpriteRenderer sr = this.GetComponent<SpriteRenderer>();

        while(timer < time)
        {
            float percent = timer / time;
            float sinPercent = Mathf.Sin(percent * percent * percent * Mathf.Rad2Deg); //Percent^3 to shorten period as we go.

            //Debug.Log(sinPercent);

            sr.color = Color.Lerp(startColor, flashColor, Mathf.Abs(sinPercent));
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1.0f);

            timer += (IsPaused) ? 0.0f : Time.deltaTime;
            yield return null;
        }

        sr.color = flashColor;

        yield return new WaitForSeconds(explosionDelay);

        //TODO: Add an explosion effect to the ParticleManager.
        GameObject explosion = GameObject.Instantiate(Resources.Load("Explosion")) as GameObject;
        explosion.transform.position = this.transform.position;
        explosion.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        ParticleSystem[] systems = explosion.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem sys in systems)
        {
            Vector3 scale = sys.gameObject.transform.localScale;
            scale *= Random.Range(0.9f, 1.1f);
            sys.gameObject.transform.localScale = scale;
        }
        explosion.GetComponent<Explosion>().triggerer = this.triggerer;

        Destroy(this.gameObject);
    }
}
