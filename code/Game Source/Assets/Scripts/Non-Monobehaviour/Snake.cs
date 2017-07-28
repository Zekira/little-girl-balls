using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake {

    public Transform[] bullets;

    public Snake(Transform[] bullets) {
        if (bullets.Length == 1) {
            bullets[0].GetComponent<Bullet>().relatedSnake = this;
            bullets[0].GetComponent<Bullet>().relatedSnakeIndex = 0;
            bullets[0].GetComponent<SpriteRenderer>().sprite = GlobalHelper.snakeSprites[0];
        } else if (bullets.Length != 0) {
            //Loop through the first half and update both ends from ends to middle
            int j = 0;
            for (int i = 0; i < (bullets.Length / 2) + 1; i++) {
                j = bullets.Length - i - 1;
                bullets[i].GetComponent<Bullet>().relatedSnake = this;
                bullets[j].GetComponent<Bullet>().relatedSnake = this;
                bullets[i].GetComponent<Bullet>().relatedSnakeLength = bullets.Length;
                bullets[j].GetComponent<Bullet>().relatedSnakeLength = bullets.Length;
                bullets[i].GetComponent<Bullet>().relatedSnakeIndex = i;
                bullets[j].GetComponent<Bullet>().relatedSnakeIndex = j;
                //Set the sprites TODO: Doing it like this is inefficient; no need to keep setting the middle sprites to the same thing over and over. Also the ends are buggy
                if (i < 3) {
                    bullets[i].GetComponent<SpriteRenderer>().sprite = GlobalHelper.snakeSprites[i];
                    bullets[j].GetComponent<SpriteRenderer>().sprite = GlobalHelper.snakeSprites[6 - i];
                } else {
                    bullets[i].GetComponent<SpriteRenderer>().sprite = GlobalHelper.snakeSprites[3];
                    bullets[j].GetComponent<SpriteRenderer>().sprite = GlobalHelper.snakeSprites[3];
                }
            }
        }
        this.bullets = bullets;
    }

    public Snake[] Split(int splitIndex) {
        if (splitIndex >= bullets.Length) {
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
            foreach (Transform t in bullets) {
                Debug.Log(t.GetComponent<Bullet>().relatedSnakeIndex); //Seems like the last one has +[snakelength] on its relatedSnakeIndex TODO
            }
            return new Snake[] { this };
        }
        Transform[] bullets1 = new Transform[index];
        Transform[] bullets2 = new Transform[bullets.Length - index - 1];
        for (int i = 0; i < bullets.Length; i++) {
            if (i < index) {
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
}
