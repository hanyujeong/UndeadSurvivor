using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;

    float timer;
    Player player;

    private void Awake()
    {
        player = GameManager.instance.player;
    }

    void Update()
    {
        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            default:
                timer += Time.deltaTime;

                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }

        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(10, 1);
        }

    }


    public void LevelUp(float damage, int count)
    {
        this.damage += damage;
        this.count += count;

        if (id == 0)

            Batch();

    }

    public void Init(ItemData data)
    {

        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        id = data.itemId;
        damage = data.baseDamage;
        count = data.baseCount;

        for (int index = 0; index < GameManager.instance.pool.GetPoolPrefabsCount(); ++index)
        {
            if (data.projectile == GameManager.instance.pool.GetPoolPrefabsOrigin(index))
            {
                prefabId = index;
                break;
            }
        }
        
        switch (id)
        {
            case 0:
                speed = 150;
                Batch();
                break;
            default:
                speed = 0.3f;
                break;

        }

    }

    void Batch()
    {
        for (int i = 0; i < count; i++)
        {
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.parent = transform;

            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * (360f / count) * i;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);
            bullet.GetComponent<Bullet>().Init(damage, -1, Vector3.zero);
        }
    }

    void Fire()
    {
        if (!player.scanner.nearestTarget)

            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position);
        dir = dir.normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform; bullet.parent = transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);

    }
}