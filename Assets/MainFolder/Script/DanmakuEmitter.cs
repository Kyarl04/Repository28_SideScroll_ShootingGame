using UnityEngine;

public class DanmakuEmitter : MonoBehaviour
{
    public GameObject bulletPrefab; // 탄막 프리팹
    public float speed = 5f;
    public int bulletCount = 12; // 탄막 개수

    // 호출 시 원형으로 탄막 발사
    public void FireRadialPattern()
    {
        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float bulDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float bulDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);

            Vector3 bulMoveVector = new Vector3(bulDirX, bulDirY, 0f);
            Vector2 bulDir = (bulMoveVector - transform.position).normalized;

            // 탄환 생성 및 발사
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = bulDir * speed;

            angle += angleStep;
        }
    }
}