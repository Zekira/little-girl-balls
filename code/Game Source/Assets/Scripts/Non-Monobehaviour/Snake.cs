using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake {

    public Transform[] bullets;
    public static Bullet bulleti, bulletj;

    public Snake(Transform[] bullets) {
        //bullets = GlobalHelper.RemoveInactive(bullets); this is a bandaid that does not fix the underlying problem which i havent even found yet
        if (bullets.Length == 1) {
            bullets[0].GetComponent<Bullet>().relatedSnake = this;
            bullets[0].GetComponent<Bullet>().relatedSnakeIndex = 0;
            bullets[0].GetComponent<Bullet>().SetSprite(GlobalHelper.snakeSprites[0]);
        } else if (bullets.Length != 0) {
            //Loop through the first half and update both ends from ends to middle
            int j = 0;
            for (int i = 0; i < (bullets.Length + 1) / 2; i++) {
                j = bullets.Length - i - 1;
                bulleti = bullets[i].GetComponent<Bullet>();
                bulletj = bullets[j].GetComponent<Bullet>();
                bulleti.relatedSnake = this;
                bulletj.relatedSnake = this;
                //Set the sprites
                if (i < 3) {
                    if (bulleti.relatedSnakeIndex != i-1)  //Prevent an already set head-sprite to being set to exactly the same again.
                        bulleti.SetSprite(GlobalHelper.snakeSprites[i]);
                    
                    if (bulletj.relatedSnakeIndex != j-1)
                        bulletj.SetSprite(GlobalHelper.snakeSprites[6 - i]);
                } else {
                    if (bulleti.relatedSnakeIndex < 2 || bulleti.relatedSnakeIndex > bullets.Length - 3) {
                        bulleti.SetSprite(GlobalHelper.snakeSprites[3]);
                    }
                    if (bulletj.relatedSnakeIndex < 2 || bulletj.relatedSnakeIndex > bullets.Length - 3) {
                        bulletj.SetSprite(GlobalHelper.snakeSprites[3]);
                    }
                }
                //Set the new indices (over here because the old ones are needed for setting the sprites above)
                bulleti.relatedSnakeIndex = i;
                bulletj.relatedSnakeIndex = j;
            }
        }
        this.bullets = bullets;
    }

    public Snake[] Split(int splitIndex) {
        if (splitIndex >= bullets.Length || splitIndex < 0) {
            //Can't split
            return new Snake[] { this };
        }
        if (splitIndex == 0 || splitIndex == bullets.Length - 1) {
            //Useless split
            return new Snake[] { this };
        }
        Transform[] bullets1 = new Transform[splitIndex];
        Transform[] bullets2 = new Transform[bullets.Length - splitIndex];
        for (int i = 0; i < bullets.Length; i++) {
            if (i < splitIndex) {
                bullets1[i] = bullets[i];
            } else {
                bullets2[i - splitIndex] = bullets[i];
            }
        }
        return new Snake[] { new Snake(bullets1), new Snake(bullets2) };
    }

    //Removes an entry and optionally splits it
    public Snake[] Remove(int index) {
        if (index < 0 || index >= bullets.Length) {
            return new Snake[] { this }; //can't remove
        }
        Transform[] bullets1 = new Transform[index];
        Transform[] bullets2 = new Transform[bullets.Length - index - 1];
        for (int i = 0; i < bullets.Length; i++) {
            if (i < index) {
                if (!bullets[i].gameObject.activeSelf) {
                    Debug.Log(bullets[i].GetComponent<Bullet>().relatedSnakeIndex + " relindex:" + index);
                }
                bullets1[i] = bullets[i];
            } else if (i > index) {
                bullets2[i - index - 1] = bullets[i];
            }
        }
        if (bullets1.Length == 0) {
            return new Snake[] { new Snake(bullets2) };
        } else if (bullets2.Length == 0) {
            return new Snake[] { new Snake(bullets1) };
        } else {
            return new Snake[] { new Snake(bullets1), new Snake(bullets2) };
        }
    }

    public Snake Add(Transform[] bullets) {
        if (this.bullets.Length == 0) {
            return new Snake(bullets);
        }
        if (bullets.Length == 0) {
            return new Snake(this.bullets);
        }
        Transform[] newBullets = new Transform[bullets.Length + this.bullets.Length];
        for (int i = 0; i < newBullets.Length; i++) {
            if (i < this.bullets.Length) {
                newBullets[i] = this.bullets[i];
            } else {
                newBullets[i] = bullets[i - this.bullets.Length];
            }
        }
        return new Snake(newBullets);
    }

    public Snake Add(Transform bullet) {
        Transform[] newBullets = new Transform[bullets.Length + 1];
        int i = 0;
        for (; i < bullets.Length; i++) {
            newBullets[i] = bullets[i];
        }
        newBullets[i] = bullet;
        return new Snake(newBullets);
    }
}
