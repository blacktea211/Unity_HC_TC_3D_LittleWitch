using UnityEngine;

public class Magic : MonoBehaviour
{
    public float attack;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "怪物") other.gameObject.GetComponent<Enemy>().Damage(attack);
        Destroy(gameObject);
    }
}
