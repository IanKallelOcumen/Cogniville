using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;           
using UnityEngine.UI;  

public enum EnemyDifficulty { Easy, Medium, Hard, Boss }

[System.Serializable]
public class EnemyProfile
{
    public string name;
    public int maxHP;             
    public EnemyDifficulty difficulty;
    
    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite attackSprite;
    public Sprite hurtSprite;
    
    // Allows you to make Bosses bigger than Goblins
    public float individualSizeModifier = 1.0f; 
}

public class QuizBattle : MonoBehaviour
{
    [Header("--- 1. CONFIGURATION ---")]
    public List<EnemyProfile> enemies; 
    private int currentEnemyIndex = 0;

    [Header("--- 2. PLAYER SPRITES ---")]
    public Sprite playerIdle;
    public Sprite playerAttack;
    public Sprite playerHurt;

    [Header("--- 3. UI ELEMENTS ---")]
    public Image[] playerHeartImages; 
    public Image[] enemyHeartImages;  
    public Sprite fullHeart, halfHeart, emptyHeart;  
    
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI questionTextUI;
    public Button[] answerButtons;        
    public TextMeshProUGUI[] buttonTexts; 

    [Header("--- 4. MOVEMENT & IMAGES ---")]
    public Transform playerObject;       
    public Image playerImageComponent;   
    
    public Transform enemyObject;        
    public Image enemyImageComponent;    
    
    // --- SIZE SLIDERS (Use these to shrink big images!) ---
    [Header("--- 5. SIZING CONTROLS ---")]
    [Range(0.1f, 2f)] public float playerSize = 0.5f; // Try 0.5 if they are too big
    [Range(0.1f, 2f)] public float enemySize = 0.5f;  // Try 0.5 if they are too big

    // --- LOGIC VARIABLES ---
    private float playerHP;
    private int currentEnemyHP;
    private string correctAnsString;
    private EnemyDifficulty currentDifficulty;

    private Vector3 playerStartPos;
    private Vector3 enemyStartPos;
    private bool isAnimating = false; 

    void Start()
    {
        // 1. AUTO-FIX HEARTS
        foreach(Image heart in playerHeartImages) if(heart) heart.preserveAspect = true;
        foreach(Image heart in enemyHeartImages) if(heart) heart.preserveAspect = true;

        // 2. SETUP PLAYER
        if (playerObject != null && playerImageComponent != null)
        {
            playerImageComponent.preserveAspect = false; 
            playerImageComponent.sprite = playerIdle;
            
            // Fix Shape & Apply Size
            playerImageComponent.SetNativeSize(); 
            playerObject.localScale = Vector3.one * playerSize;

            playerStartPos = playerObject.position;
        }

        // 3. SETUP ENEMY
        if (enemyObject != null && enemyImageComponent != null)
        {
            enemyImageComponent.preserveAspect = false; 
            enemyStartPos = enemyObject.position;
        }

        playerHP = playerHeartImages.Length; 
        LoadEnemy(0);
    }

    void Update()
    {
        // IDLE ANIMATION: Float + Breathe
        // We use 'playerSize' and 'enemySize' as the base numbers here
        if (!isAnimating && playerObject != null && enemyObject != null)
        {
            float time = Time.time;
            
            float floatX = Mathf.Sin(time * 2f) * 0.1f; 
            float breathY = 1f + Mathf.Sin(time * 3f) * 0.02f;
            float breathX = 1f - Mathf.Sin(time * 3f) * 0.01f;

            // Apply to Player
            playerObject.position = playerStartPos + new Vector3(floatX, 0, 0);
            playerObject.localScale = new Vector3(playerSize * breathX, playerSize * breathY, 1f);

            // Apply to Enemy
            // Note: We use the enemy's individual modifier too!
            float finalEnemySize = enemySize * enemies[currentEnemyIndex].individualSizeModifier;
            enemyObject.position = enemyStartPos + new Vector3(-floatX, 0, 0);
            enemyObject.localScale = new Vector3(finalEnemySize * breathX, finalEnemySize * breathY, 1f);
        }
    }

    void LoadEnemy(int index)
    {
        if (index >= enemies.Count) return;

        currentEnemyIndex = index;
        EnemyProfile activeEnemy = enemies[currentEnemyIndex];
        currentDifficulty = activeEnemy.difficulty;
        currentEnemyHP = activeEnemy.maxHP;

        // Set Sprite, Fix Shape, Apply Size
        enemyImageComponent.sprite = activeEnemy.idleSprite;
        enemyImageComponent.SetNativeSize(); 
        
        float finalSize = enemySize * activeEnemy.individualSizeModifier;
        enemyObject.localScale = Vector3.one * finalSize;

        enemyNameText.text = activeEnemy.name;
        UpdateGUI(); 
        GenerateMathQuestion();
    }

    void GenerateMathQuestion()
    {
        foreach(Button b in answerButtons) b.interactable = true;

        int num1 = 0, num2 = 0, correctAns = 0;
        string symbol = "+";

        switch (currentDifficulty)
        {
            case EnemyDifficulty.Easy:
                num1 = Random.Range(1, 10); num2 = Random.Range(1, 10);
                symbol = "+"; correctAns = num1 + num2;
                break;
            case EnemyDifficulty.Medium:
                num1 = Random.Range(10, 30); num2 = Random.Range(1, 15);
                symbol = (Random.value > 0.5f) ? "+" : "-";
                correctAns = (symbol == "+") ? num1 + num2 : num1 - num2;
                break;
            case EnemyDifficulty.Hard:
                num1 = Random.Range(2, 10); num2 = Random.Range(2, 10);
                symbol = "x"; correctAns = num1 * num2;
                break;
            case EnemyDifficulty.Boss:
                num1 = Random.Range(5, 12); num2 = Random.Range(5, 10);
                symbol = "x"; correctAns = num1 * num2;
                break;
        }

        questionTextUI.text = $"{num1} {symbol} {num2} = ?";
        correctAnsString = correctAns.ToString();
        SetupAnswerButtons(correctAns);
    }

    void SetupAnswerButtons(int correctAns)
    {
        List<string> options = new List<string>();
        options.Add(correctAns.ToString());

        while (options.Count < 3) 
        {
            int fake = correctAns + Random.Range(-5, 6);
            if (fake != correctAns && !options.Contains(fake.ToString()))
                options.Add(fake.ToString());
        }

        for (int i = 0; i < options.Count; i++) {
            string temp = options[i];
            int rnd = Random.Range(i, options.Count);
            options[i] = options[rnd];
            options[rnd] = temp;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < options.Count)
            {
                buttonTexts[i].text = options[i];
                answerButtons[i].onClick.RemoveAllListeners();
                string myAnswer = options[i];
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(myAnswer));
            }
        }
    }

    void OnAnswerSelected(string chosen)
    {
        foreach(Button b in answerButtons) b.interactable = false;
        
        isAnimating = true; 

        EnemyProfile activeEnemy = enemies[currentEnemyIndex];
        float finalEnemySize = enemySize * activeEnemy.individualSizeModifier;

        // FORCE SIZE RESET
        playerObject.localScale = Vector3.one * playerSize;
        enemyObject.localScale = Vector3.one * finalEnemySize;

        if (chosen == correctAnsString)
        {
            // CORRECT
            currentEnemyHP--; 
            Vector3 target = enemyStartPos - new Vector3(2.0f, 0, 0); 
            
            StartCoroutine(PerformAttackSequence(
                playerObject, playerStartPos, target, playerImageComponent, playerAttack, playerIdle, playerSize, // Attacker
                enemyObject, enemyStartPos, enemyImageComponent, activeEnemy.hurtSprite, activeEnemy.idleSprite, finalEnemySize // Victim
            ));
        }
        else
        {
            // WRONG
            float damage = 0;
            switch(currentDifficulty) {
                case EnemyDifficulty.Easy: damage = 0.5f; break;
                case EnemyDifficulty.Medium: damage = 1.0f; break;
                case EnemyDifficulty.Hard: damage = 1.5f; break;
                case EnemyDifficulty.Boss: damage = 2.0f; break;
            }
            playerHP -= damage;
            Vector3 target = playerStartPos + new Vector3(2.0f, 0, 0);
            
            StartCoroutine(PerformAttackSequence(
                enemyObject, enemyStartPos, target, enemyImageComponent, activeEnemy.attackSprite, activeEnemy.idleSprite, finalEnemySize, // Attacker
                playerObject, playerStartPos, playerImageComponent, playerHurt, playerIdle, playerSize // Victim
            ));
        }
    }

    IEnumerator PerformAttackSequence(
        Transform attackerObj, Vector3 attackerHome, Vector3 attackPos, Image attackerImg, Sprite attackSprite, Sprite attackerIdle, float attackerScale,
        Transform victimObj, Vector3 victimHome, Image victimImg, Sprite victimHurtSprite, Sprite victimIdle, float victimScale)
    {
        float speed = 0.1f; 

        attackerObj.localScale = Vector3.one * attackerScale;
        victimObj.localScale = Vector3.one * victimScale;

        // 1. CHANGE SPRITE & RESIZE BOX
        if (attackerImg) {
            attackerImg.sprite = attackSprite;
            attackerImg.SetNativeSize(); 
        }

        // 2. ANTICIPATION
        float timer = 0;
        Vector3 windUpPos = attackerHome + (attackerHome - attackPos).normalized * 0.5f;
        while (timer < 0.1f)
        {
            attackerObj.position = Vector3.Lerp(attackerHome, windUpPos, timer / 0.1f);
            timer += Time.deltaTime;
            yield return null;
        }

        // 3. LUNGE
        timer = 0;
        while (timer < speed)
        {
            attackerObj.position = Vector3.Lerp(windUpPos, attackPos, timer / speed);
            timer += Time.deltaTime;
            yield return null;
        }
        attackerObj.position = attackPos; 

        // 4. IMPACT
        if (victimImg) {
            victimImg.sprite = victimHurtSprite;
            victimImg.SetNativeSize(); 
            victimImg.color = Color.red;
        }

        for(int i = 0; i < 6; i++)
        {
            Vector3 shake = Random.insideUnitCircle * 0.3f;
            victimObj.position = victimHome + shake;
            yield return new WaitForSeconds(0.03f);
        }
        
        yield return new WaitForSeconds(0.1f); 

        // 5. RESET COLORS
        if (victimImg) victimImg.color = Color.white;

        // 6. RETURN HOME
        timer = 0;
        while (timer < speed)
        {
            attackerObj.position = Vector3.Lerp(attackPos, attackerHome, timer / speed);
            victimObj.position = Vector3.Lerp(victimObj.position, victimHome, timer / speed);
            timer += Time.deltaTime;
            yield return null;
        }

        // 7. RESET EVERYTHING
        attackerObj.position = attackerHome;
        victimObj.position = victimHome;
        
        if (attackerImg) {
            attackerImg.sprite = attackerIdle;
            attackerImg.SetNativeSize(); 
        }
        if (victimImg) {
            victimImg.sprite = victimIdle;
            victimImg.SetNativeSize(); 
        }

        // Force scale one last time
        attackerObj.localScale = Vector3.one * attackerScale;
        victimObj.localScale = Vector3.one * victimScale;

        isAnimating = false;
        
        UpdateGUI();
        CheckGameState();
    }

    void UpdateGUI()
    {
        for (int i = 0; i < playerHeartImages.Length; i++)
        {
            if (playerHP >= (i + 1)) playerHeartImages[i].sprite = fullHeart;
            else if (playerHP > i) playerHeartImages[i].sprite = halfHeart;
            else playerHeartImages[i].sprite = emptyHeart;
        }

        for (int i = 0; i < enemyHeartImages.Length; i++)
        {
            if (i < currentEnemyHP) {
                enemyHeartImages[i].enabled = true;
                enemyHeartImages[i].sprite = fullHeart;
            } else {
                enemyHeartImages[i].enabled = false;
            }
        }
    }

    void CheckGameState()
    {
        if (playerHP <= 0) questionTextUI.text = "GAME OVER";
        else if (currentEnemyHP <= 0)
        {
            if (currentEnemyIndex < enemies.Count - 1) LoadEnemy(currentEnemyIndex + 1);
            else questionTextUI.text = "VICTORY!";
        }
        else GenerateMathQuestion();
    }
}