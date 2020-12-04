using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperGun : MonoBehaviour {
    // inner private class that may be exposed (public) someday, somewhere else...
    class Interpolated : MonoBehaviour {   

        Vector3 previousPosition;
        Quaternion previousRotation;
        float previousTime;

        public void Snapshot() {
            previousPosition = transform.position;
            previousRotation = transform.rotation;
            previousTime = Time.time;
        }

        void Start() {
            hideFlags = HideFlags.HideInInspector;
            Snapshot();
        }

        public (Vector3 position, Quaternion rotation) GetInterpolatedPositionAndRotation(float deltaTime) {
            var t = 1f - deltaTime / (Time.time - previousTime);
            var p = Vector3.Lerp(previousPosition, transform.position, t);
            var q = Quaternion.Slerp(previousRotation, transform.rotation, t);
            return (p, q);
        }
    }

    public Item bulletSource;
    [Utils.Layer]
    public int bulletLayer;

    [Header("SuperGun")]
    public float frequence = 30f;
    public string pattern = "1110";
    [Tooltip("Le canon tourne-t-il sur lui même ?")]
    public float angularVelocity = 0f;

    [Header("Bullet")]
    public float bulletLifeMax = 1f;
    public float bulletVelocity = 20f;
    public float bulletAngularVelocityZ = 0f;

    [Space(16)]
    public bool hideBulletsInHierarchy = true;

    [Header("Gizmos")]
    public Color gizmoColor = new Color(1f, 1f, 0f);
    public float gizmoScale = 1f;

    int count = 0;
    float timer = 0;
    Interpolated interpolated;

    void Start() {
        interpolated = gameObject.AddComponent<Interpolated>();
    }

    public void Fire(float deltaTime = 0f) {

        if (bulletSource != null) {

            var (position, rotation) = interpolated.GetInterpolatedPositionAndRotation(deltaTime);
            var bullet = Instantiate(bulletSource, position, rotation);
            bullet.gameObject.hideFlags = hideBulletsInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;
            bullet.gameObject.layer = bulletLayer;
            bullet.timeMax = bulletLifeMax;
            bullet.velocity = (rotation * Vector3.right) * bulletVelocity;
            bullet.angularVelocity = new Vector3(0f, 0f, bulletAngularVelocityZ);
            bullet.name = string.Format("{0} F{1}-#{2}-t{3}", bullet.name, Time.frameCount, count, deltaTime.ToString("F3"));

            // subframe update
            bullet.transform.position += bullet.velocity * deltaTime;
            bullet.transform.rotation *= Quaternion.Euler(bullet.angularVelocity * deltaTime);
        }
    }

    void FixedUpdate() {

        transform.rotation *= Quaternion.Euler(0f, 0f, angularVelocity * Time.fixedDeltaTime);

        timer += Time.fixedDeltaTime * Item.timeScale;
        float period = 1f / frequence;

        while (timer > period) {

            timer += -period;

            var c = pattern[count % pattern.Length];
            if (c == '1') {
                Fire(timer);
            }

            count++;
        }
        
        interpolated.Snapshot();
    }

    void OnDrawGizmos() {

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.075f * gizmoScale);
        Gizmos.DrawRay(transform.position, transform.right * gizmoScale);
    }
}

