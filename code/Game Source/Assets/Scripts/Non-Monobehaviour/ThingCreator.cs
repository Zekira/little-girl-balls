using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class handling the creation/instantation of bullets/enemies/etc
/// </summary>
public static class ThingCreator {
    //Things needed when creating stuff
    private static GameObject createdObject;
    private static Bullet bullet;
    private static Transform bulletTransform;
    private static SpriteRenderer spriteRenderer;
    private static MaterialPropertyBlock bulletMatPropertyBlock = new MaterialPropertyBlock();
    private static Vector3 smallItemSize = new Vector3(0.45f, 0.45f, 1f);

    /// <summary>
    /// Creates an enemy from a template with appropriate settings, applies the EnemyTemplate to the created object's Enemy class, and returns the created object.
    /// </summary>
    public static GameObject CreateEnemy(EnemyTemplate enemyTemplate) {
        //No regular enemies are allowed to be created when there is a boss on screen.
        if (GlobalHelper.activeBosses > 0 && !enemyTemplate.isBoss) {
            return null;
        }
        //Create the object and set the settings.
        createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Enemy"));

        createdObject.transform.position += new Vector3(enemyTemplate.startpostion.x, enemyTemplate.startpostion.y, 0f);
        createdObject.transform.localScale = enemyTemplate.scale * Vector3.one;

        createdObject.GetComponent<Enemy>().health = enemyTemplate.maxHealth;

        createdObject.transform.GetComponent<SpriteAnimator>().SetSprites(GlobalHelper.enemySprites[enemyTemplate.enemyID]);

        createdObject.GetComponent<Enemy>().template = enemyTemplate;
        return createdObject;
    }

    /// <summary>
    /// Creates a bullet from a template with appropriate settings, applies the BulletTemplate to the created object's Bullet class, and returns the created object.
    /// </summary>
    /// <returns>Returns a reference to the created bullet.</returns>
    public static GameObject CreateBullet(BulletTemplate bulletTemplate, Vector2 bulletPosition) {
        GlobalHelper.currentBullets++;
        //The z-value of bullets is this value because this prevents z-fighting. Also, fun stats.
        GlobalHelper.totalFiredBullets++;
        //Take it either from the backup list, or instantiate a new bullet. The former is prefered because it's faster.
        if (GlobalHelper.backupBullets.Count == 0) {
            createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Bullet"));
        } else {
            int index = GlobalHelper.backupBullets.Count - 1;
            createdObject = GlobalHelper.backupBullets[index];
            GlobalHelper.backupBullets.RemoveAt(index);
            createdObject.SetActive(true);
        }
        bullet = createdObject.GetComponent<Bullet>();
        bullet.Reset(bulletTemplate);
        createdObject.GetComponent<TimelineInterprenter>().enabled = false;
        //Sets the position
        if (!bulletTemplate.positionIsRelative) {
            bullet.posx = bulletTemplate.position.x;
            bullet.posy = bulletTemplate.position.y;
        } else {
            bullet.posx = bulletPosition.x + bulletTemplate.position.x;
            bullet.posy = bulletPosition.y + bulletTemplate.position.y;
        }
        bullet.posz = GlobalHelper.totalFiredBullets * 1e-6f;
        if (!bulletTemplate.enemyShot) {
            bullet.posz += 5f; //Player shot bullets should not cover actual harmful bullets.
        }
        //Set the actual position
        bulletTransform = createdObject.transform;
        bulletTransform.position = new Vector3(bullet.posx, bullet.posy, bullet.posz);

        bulletTransform.localScale = bulletTemplate.scale * Vector3.one;
        bulletTransform.eulerAngles = new Vector3(0f, 0f, -bulletTemplate.rotation * Mathf.Rad2Deg);

        spriteRenderer = createdObject.GetComponent<SpriteRenderer>();
        bullet.SetSpriteDirectly(GlobalHelper.bulletSprites[bulletTemplate.bulletID]);
        //If the property block is empty, initalise it here because it's needed.
        spriteRenderer.GetPropertyBlock(bulletMatPropertyBlock);
        //Change the color the sprites should render
        bulletMatPropertyBlock.SetColor("_Color1", bulletTemplate.innerColor);
        bulletMatPropertyBlock.SetColor("_Color2", bulletTemplate.outerColor);
        spriteRenderer.SetPropertyBlock(bulletMatPropertyBlock);
        //Start the animation of the bullet spawning by disabling the Bullet thing and enabling the Materialisation.
        bullet.enabled = false;
        createdObject.GetComponent<BulletMaterialisation>().enabled = true;
        return createdObject;
    }

    /// <summary>
    /// Creates an empty bullet ready to be used by CreateBullet(*actual arguments*). Spam this when not lagging. This is done to prevent Instantiate() lagginess as flipping a bool is faster than that.
    /// </summary>
    public static void CreateEmptyBullet() {
        GlobalHelper.currentBullets++;
        createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Bullet"));
        createdObject.GetComponent<Bullet>().Deactivate();
    }

    public static GameObject CreateItem(Item.ItemType type, Vector3 position) {
        if (GlobalHelper.backupItems.Count == 0) {
            createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Item"));
        } else {
            createdObject = GlobalHelper.backupItems[0];
            GlobalHelper.backupItems.RemoveAt(0);
            createdObject.SetActive(true);
        }
        Item item = createdObject.GetComponent<Item>();
        item.type = type;
        switch (type) {
            case Item.ItemType.POINT:
            case Item.ItemType.POWER:
                createdObject.transform.localScale = smallItemSize;
                break;
            default: //Fullpower, largepower, 
                createdObject.transform.localScale = Vector3.one;
                break;
        }
        //TODO: Update this whenever a new texture is added to Resources/Graphics/Items
        switch (type) {
            case Item.ItemType.FULLPOWER:
                createdObject.GetComponent<SpriteRenderer>().sprite = GlobalHelper.itemSprites[0];
                break;
            case Item.ItemType.POINT:
                createdObject.GetComponent<SpriteRenderer>().sprite = GlobalHelper.itemSprites[1];
                break;
            case Item.ItemType.POWER:
            case Item.ItemType.LARGEPOWER:
                createdObject.GetComponent<SpriteRenderer>().sprite = GlobalHelper.itemSprites[2];
                break;
        }
        createdObject.transform.position = position;
        return createdObject;
    }

    public static GameObject CreateLaser(LaserTemplate template, Vector2 position) {
        createdObject = GameObject.Instantiate(Resources.Load("Prefabs/Laser") as GameObject);
        if (template.positionIsRelative) {
            createdObject.transform.position = new Vector3(position.x + template.position.x, position.y + template.position.y, 1);
        } else {
            createdObject.transform.position = new Vector3(template.position.x, template.position.y, 1f);
        }
        createdObject.transform.localScale = new Vector3(0.06f, 99f, 1f);
        createdObject.GetComponent<Laser>().template = template;

        spriteRenderer = createdObject.transform.GetComponent<SpriteRenderer>();
        //If the property block is empty, initalise it here because it's needed.
        spriteRenderer.GetPropertyBlock(bulletMatPropertyBlock);
        //Change the color the sprites should render
        bulletMatPropertyBlock.SetColor("_Color1", template.innerColor);
        bulletMatPropertyBlock.SetColor("_Color2", template.outerColor);
        spriteRenderer.SetPropertyBlock(bulletMatPropertyBlock);

        return createdObject;
    }

    //Can't coroutine in a static or non-monobehaviour class. So that's great.
    public static void CreateSnake(int length, BulletTemplate template, Vector3 position) {
        GlobalHelper.thisHelper.CreateSnake(length, template, position);
    }
}
