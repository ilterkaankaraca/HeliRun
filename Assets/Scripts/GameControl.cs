﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public Image gameOverImage;
    public Text clickToStart;
    public GameObject background1;
    public GameObject background2;
    public GameObject block;
    public int blockNumber=5;
    public static float backgroundSpeed = 5f;
    public Image title;
    public GameObject helicopter;
    public Text highscoreText;
    public static bool gameOver = false;
    public static GameObject[] blocks;
    public static Rigidbody2D bgrigid1;
    public static Rigidbody2D bgrigid2;
    public static Rigidbody2D blockRigid;
    private float reset_time = 0;
    int counter = 0;
    private float size = 0;
    public static int score=0; 
    public static int highscore; 
    public static bool gameStarted = false;
    void Start()
    {
        score = 0; 
        heliControl.newHighScore = false;
        gameOver = false;
        gameStarted = false;
        bgrigid1 = background1.GetComponent<Rigidbody2D>();
        bgrigid2 = background2.GetComponent<Rigidbody2D>();
        blocks = new GameObject[blockNumber];
        highscore = PlayerPrefs.GetInt("highScore");
        highscoreText.text = "High Score: " + highscore.ToString();
        gameOverImage.gameObject.SetActive(false);
    }
    void WaitforInput()
    {
        if(gameStarted==false && GetVertical() > 0)
        {
            gameOver = false;
            gameStarted = true;
            StartGame();
            title.gameObject.SetActive(false);
            highscoreText.gameObject.SetActive(false);
            clickToStart.gameObject.SetActive(false);
            
        }
        else if(gameStarted==true && GetVertical()>0 && gameOver == true)
        {
            SceneManager.LoadScene("scene1");
            Start();
        }
    }
    void StartGame()
    {
        bgrigid1.velocity = new Vector2(-backgroundSpeed, 0);
        bgrigid2.velocity = new Vector2(-backgroundSpeed, 0);
        helicopter.GetComponent<Rigidbody2D>().simulated = true;
       size = background1.GetComponent<BoxCollider2D>().size.x;
        heliControl.sounds[0].Play();
        
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i] = Instantiate(block, new Vector2(-20, -20), Quaternion.Euler(0,0,270));
            blockRigid = blocks[i].AddComponent<Rigidbody2D>();
            blockRigid.gravityScale = 0;
            blockRigid.velocity = new Vector2(-backgroundSpeed, 0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        WaitforInput();
        if(background1.transform.position.x <= -size)
        {
            background1.transform.position += new Vector3(size * 2, 0);
        }
        if (background2.transform.position.x <= -size)
        {
            background2.transform.position += new Vector3(size * 2, 0);
        }
        reset_time += Time.deltaTime;
        if (reset_time > 2f && gameOver==false && gameStarted==true)
        {
            reset_time = 0;
            float yAxis = Random.Range(-8.44f, -1.91f);
            blocks[counter].transform.position = new Vector3(8f, yAxis);
            float z = Random.Range(246f, 290f);
            blocks[counter].transform.rotation = Quaternion.Euler(0, 0, z);
            counter++;
            if (counter >= blocks.Length)
            {
                counter = 0;
            }
        }
    }
    public void GameOver()
    {
        helicopter.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            bgrigid1.velocity = Vector2.zero;
            bgrigid2.velocity = Vector2.zero;
        }
        gameOverImage.gameObject.SetActive(true);
        clickToStart.gameObject.SetActive(true);
        if (heliControl.newHighScore)
        {
            highscoreText.text = "New High Score";
            highscoreText.gameObject.SetActive(true);
        }
    }   
    public static float GetVertical()
    {
        if (Input.GetMouseButton(0))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

}
