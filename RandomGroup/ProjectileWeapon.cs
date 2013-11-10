using System.Collections.Generic;
using TgcViewer.Utils._2D;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.RandomGroup
{
    public class ProjectileWeapon : Weapon
    {
        public ShootTechnique technique = new ShootTechnique();//Por default dispara una sola bala por vez
        
        protected TgcStaticSound NoAmmo = new TgcStaticSound();
        protected TgcStaticSound ShootSound = new TgcStaticSound();


        public float distance = 8;
        public int timeBetweenShoots = 500;
        public Vector3 initDir;
        public Vector3 pos;
        public Vector3 dir;
        public Vector3 initPos;
        public Projectile tmpBullet;
        TgcCamera camera;

        public ProjectileWeapon(Drawable weaponDrawing, TgcSprite sprite, string path)
        {
            this.weaponDrawing = weaponDrawing;
            crosshair = sprite;
            NoAmmo.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\CannonShoot.wav");
            ShootSound.loadSound(path);
        }

        public ProjectileWeapon setShootTechnique(ShootTechnique technique)
        {
            this.technique = technique;
            return this;
        }

        public override List<Projectile> doAction()
        {
            List<Projectile> tmpList = technique.getShoot();
            camera = GuiController.Instance.CurrentCamera;
            initPos = camera.getLookAt();
            initDir = initPos - camera.getPosition();
            initPos.Add(initDir * distance);
            Projectile tmpColisionador = new Projectile(initPos, technique.bulletDrawing.clone(), initDir);
            ShootSound.play();
            return tmpList;
        }
    }
}