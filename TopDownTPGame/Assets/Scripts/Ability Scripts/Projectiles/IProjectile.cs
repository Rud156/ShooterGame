using UnityEngine;

namespace AbilityScripts.Projectiles
{
    public interface IProjectile
    {
        public void LaunchProjectile(Vector3 direction);

        public void ProjectileHit(Collider other);

        public void ProjectileDestroy();
    }
}