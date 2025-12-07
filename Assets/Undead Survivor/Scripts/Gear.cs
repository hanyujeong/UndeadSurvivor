using UnityEngine;

public class Gear : MonoBehaviour
{
    public ItemData.ItemType type;
    public float rate;

    public void Init(ItemData data)
    {
        name = "Gear " + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;
        type = data.itemType;
        rate = data.damages[0];
    }

    public void LevelUp(float rate)
    {
        this.rate = rate;
    }

    void RateUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();

        foreach (Weapon weapon in weapons)
        {
            switch (weapon.id) {   
                case 0:
                    weapon.speed = 150 + (150 * rate );
                    break;
               default:
                    weapon.speed = 0.5f * (1f - rate );
                    break;
            }

        }
    }

}
