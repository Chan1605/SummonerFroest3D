using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Hero : MonoBehaviour
{
    public enum YasuoState
    {
        idle, trace, attack, hit, die, skill, skilling, skillend
    }

    public YasuoState yasuo = YasuoState.idle;

    [Header("Status")]
    [SerializeField] private float m_CurHp = 100.0f;
    [SerializeField] private float m_MaxHp = 100.0f;
    public float m_MoveVelocity = 8.0f;

    [Header("UI")]
    public Image hpbar;
    public RectTransform Aim;
    public Image AimF;
    public GameObject TrEff;

    [Header("Auto Attack / Kiting")]
    [SerializeField] private float m_AttackRange = 6.0f;
    [SerializeField] private float m_AttackCooldown = 0.7f;
    [SerializeField] private float m_AttackDamage = 20.0f;
    [SerializeField] private float m_AttackMoveSearchRadius = 3.0f;

    [Header("Attack")]
    [SerializeField] private float m_AttackDist = 1.9f;
    [SerializeField] private float m_RotSpeed = 7.0f;

    [Header("Skill Cooltime")]
    [SerializeField] private float skill_Time = 3.0f;
    [SerializeField] private float Dskill_Time = 15.0f;
    [SerializeField] private float Fskill_Time = 20.0f;

    [Header("Skill Effects")]
    public GameObject SkillEffect;
    public GameObject SkillEnd;
    public GameObject HealEffect;
    public GameObject FlashEffect;
    public GameObject Trail;

    [Header("Weapon")]
    public GameObject Sword;

    // Picking
    private Ray a_MousePos;
    private RaycastHit hitInfo;
    private LayerMask m_layerMask = -1;

    // Move
    private bool m_isPickMvOnOff = false;
    private Vector3 m_TargetPos = Vector3.zero;
    private Vector3 m_MoveDir = Vector3.zero;
    private double m_MoveDurTime = 0.0;
    private double m_AddTimeCount = 0.0;
    private Vector3 a_StartPos = Vector3.zero;
    private Vector3 a_CacLenVec = Vector3.zero;
    private Quaternion a_TargetRot;

    // Target / Attack
    private GameObject m_TargetUnit = null;
    private Vector3 a_CacTgVec = Vector3.zero;
    private Vector3 a_CacAtDir = Vector3.zero;
    private float a_CacRotSpeed = 0.0f;
    private float m_AttackTimer = 0.0f;
    private bool m_IsAttackCommandMode = false;
    private bool m_IsAutoAttacking = false;

    // Components
    private Animator m_RefAnimator = null;
    private BoxCollider SwordCol;
    private TrailRenderer AttTrEff;
    private ColorCorrectionCurves colorCorrection;
    private Transform Taget;

    // State flags
    private bool IsSkill = false;
    [HideInInspector] public bool IsBuff = false;
    [HideInInspector] public bool IsDie = false;

    // Skill timers
    private float Wskill_Time = 0.0f;
    private float WDuration = 0.0f;
    private float skill_Delay = 0.0f;
    private float Dskill_Delay = 0.0f;
    private float Fskill_Delay = 0.0f;
    private float GuideTimer = 0.0f;

    // Effect instances
    private GameObject Skill1;
    private GameObject HealInst;
    private GameObject FlashInst;

    // Count
    private int Skcnt = 1;
    private int Ncnt = 1;
    private int cnt;
    public int Killcount = 0;

    [HideInInspector] public int m_CurPathIndex = 1;
    public static Hero Inst = null;

    void Awake()
    {
        Cam a_CamCtrl = Camera.main.GetComponent<Cam>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);

        Inst = this;
    }

    void Start()
    {
        colorCorrection = FindObjectOfType<ColorCorrectionCurves>();

        m_CurHp = m_MaxHp;
        GameMgr.Inst.Yasuo = this;

        m_layerMask = 1 << LayerMask.NameToLayer("MyTerrain");
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit");

        AttTrEff = GameObject.Find("Katana").GetComponent<TrailRenderer>();
        m_RefAnimator = GetComponent<Animator>();
        SwordCol = Sword.GetComponent<BoxCollider>();
        SwordCol.enabled = false;

        yasuo = YasuoState.idle;
        EnemyCheck();
        UpdateHpUI();
    }

    void Update()
    {
        if (IsDie)
            return;

        UpdateAttackTimer();
        AttackCommandInput();
        MousePick();
        MousePickUpdate();
        AutoAttackUpdate();
        YasuoActionUpdate();
        UseSkill();
        UseWSkill();
        UseFlash();
        UseHeal();
        UiInfo();
        UpdateIdleState();
    }

    // =========================
    // Update Helpers
    // =========================
    void UpdateAttackTimer()
    {
        m_AttackTimer -= Time.deltaTime;
        if (m_AttackTimer < 0.0f)
            m_AttackTimer = 0.0f;
    }

    void UpdateIdleState()
    {
        if (m_isPickMvOnOff == false &&
            IsSkill == false &&
            m_TargetUnit == null &&
            IsBuff == false &&
            m_IsAutoAttacking == false)
        {
            yasuo = YasuoState.idle;
        }
    }

    void UpdateHpUI()
    {
        hpbar.fillAmount = m_CurHp / m_MaxHp;
        GameMgr.Inst.HpInfo.text = m_CurHp + " / " + m_MaxHp;
    }

    void ShowGuide(string msg, float time = 1.0f)
    {
        GameMgr.Inst.GuideText.gameObject.SetActive(true);
        GameMgr.Inst.GuideText.text = msg;
        GuideTimer = time;
    }

    void Update_MousePosition()
    {
        Aim.position = Input.mousePosition;
    }

    void EnemyCheck()
    {
        GameMgr.Inst.EnemyTxt.text = "Kill Count : " + Killcount;
    }

    void DiaCheck()
    {
        if (Ncnt < 1)
            Ncnt = 1;

        GameMgr.Inst.SkCntTxt.text = "x " + Ncnt;
    }

    // =========================
    // Animation / State
    // =========================
    void AnimType(string anim)
    {
        if (anim != "IsAttack")
            AttTrEff.emitting = false;

        m_RefAnimator.SetBool("Idle", false);
        m_RefAnimator.SetBool("IsTrace", false);
        m_RefAnimator.SetBool("IsAttack", false);
        m_RefAnimator.SetBool("IsDie", false);
        m_RefAnimator.SetBool("IsSkill", false);

        m_RefAnimator.SetBool(anim, true);
    }

    void YasuoActionUpdate()
    {
        switch (yasuo)
        {
            case YasuoState.idle:
                AnimType("Idle");
                break;

            case YasuoState.trace:
                AnimType("IsTrace");
                break;

            case YasuoState.attack:
                AttackRotUpdate();
                AnimType("IsAttack");
                AttTrEff.emitting = true;
                EnemyCheck();
                break;

            case YasuoState.skill:
                AnimType("IsSkill");
                colorCorrection.enabled = true;
                Time.timeScale = 0.5f;
                TrEff.GetComponent<TrailRenderer>().emitting = true;
                Update_MousePosition();
                break;

            case YasuoState.skilling:
                ExecuteQSkillHit();
                break;

            case YasuoState.skillend:
                EndQSkill();
                break;
        }
    }

    void ExecuteQSkillHit()
    {
        if (Taget == null)
        {
            yasuo = YasuoState.skillend;
            return;
        }

        Vector3 dir = Taget.position - transform.position;
        dir.y = 0.0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            dir.Normalize();
            transform.forward = dir;
        }

        Vector3 tagetpos = Taget.position + (dir * 2.0f);

        Cam camCtrl = Camera.main.GetComponent<Cam>();
        if (camCtrl != null)
            camCtrl.DelayFollow(0.1f);

        Taget.GetComponent<MonCtrl>().TakeDamage(100);
        DiaCheck();
        EnemyCheck();

        transform.position = tagetpos;

        Vector3 effectpos = tagetpos;
        effectpos.y += 2.0f;
        PlayTempEffect(SkillEnd, effectpos, 1.0f, out Skill1);

        yasuo = YasuoState.skill;
    }

    void EndQSkill()
    {
        IsSkill = false;
        colorCorrection.enabled = false;
        SwordCol.enabled = false;
        Aim.gameObject.SetActive(false);
        SkillEffect.SetActive(false);
        Time.timeScale = 1.0f;
        skill_Delay = skill_Time;
        TrEff.GetComponent<TrailRenderer>().emitting = false;
        yasuo = YasuoState.idle;
    }

    // =========================
    // Auto Attack / Kiting
    // =========================
    void AttackCommandInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
            m_IsAttackCommandMode = true;
    }

    void SetAttackTarget(GameObject enemy)
    {
        if (enemy == null)
            return;

        m_TargetUnit = enemy;
        m_IsAutoAttacking = true;
        m_isPickMvOnOff = false;
    }

    void StopAutoAttack()
    {
        m_IsAutoAttacking = false;
        m_TargetUnit = null;

        if (IsSkill == false && IsBuff == false)
            yasuo = YasuoState.idle;
    }

    GameObject FindNearestEnemyInRange(Vector3 center, float searchRadius)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null || enemies[i].activeInHierarchy == false)
                continue;

            float dist = Vector3.Distance(center, enemies[i].transform.position);
            if (dist < searchRadius && dist < minDist)
            {
                minDist = dist;
                nearest = enemies[i];
            }
        }

        return nearest;
    }

    void AutoAttackUpdate()
    {
        if (m_IsAutoAttacking == false)
            return;

        if (IsSkill || IsDie)
            return;

        if (m_TargetUnit == null)
        {
            StopAutoAttack();
            return;
        }

        MonCtrl mon = m_TargetUnit.GetComponent<MonCtrl>();
        if (mon == null || m_TargetUnit.activeInHierarchy == false)
        {
            StopAutoAttack();
            return;
        }

        Vector3 toTarget = m_TargetUnit.transform.position - transform.position;
        toTarget.y = 0.0f;
        float dist = toTarget.magnitude;

        if (dist > m_AttackRange)
        {
            Vector3 dir = toTarget.normalized;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * m_RotSpeed);
            }

            transform.position += dir * Time.deltaTime * m_MoveVelocity;
            yasuo = YasuoState.trace;
        }
        else
        {
            AttackRotUpdate();

            if (m_AttackTimer <= 0.0f)
            {
                yasuo = YasuoState.attack;
                mon.TakeDamage((int)m_AttackDamage);
                m_AttackTimer = m_AttackCooldown;
            }
            else
            {
                yasuo = YasuoState.idle;
            }
        }
    }

    // =========================
    // Move / Picking
    // =========================
    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
        if (yasuo == YasuoState.skill)
            return;

        a_StartPos = transform.position;
        a_SetPickVec.y = transform.position.y;

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

        if (a_PickMon != null)
        {
            if (yasuo != YasuoState.attack)
                yasuo = YasuoState.attack;

            a_CacTgVec = a_PickMon.transform.position - transform.position;
            if (a_CacTgVec.magnitude <= m_AttackDist)
            {
                m_TargetUnit = a_PickMon;
                return;
            }
        }

        if (a_CacLenVec.magnitude < 0.5f)
            return;

        float a_PathLen = a_CacLenVec.magnitude;

        m_TargetPos = a_SetPickVec;
        m_isPickMvOnOff = true;
        m_MoveDir = a_CacLenVec.normalized;
        m_MoveDurTime = a_PathLen / m_MoveVelocity;
        m_AddTimeCount = 0.0;

        a_StartPos = transform.position;
        a_SetPickVec.y = transform.position.y;
        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;
    }

    void MousePickUpdate()
    {
        if (m_IsAutoAttacking)
            return;

        if (m_isPickMvOnOff == false)
            return;

        a_CacLenVec = m_TargetPos - transform.position;
        a_CacLenVec.y = 0.0f;

        if (0.1f < a_CacLenVec.magnitude)
        {
            m_MoveDir = a_CacLenVec.normalized;
            a_TargetRot = Quaternion.LookRotation(m_MoveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * m_RotSpeed);
        }

        m_MoveDir = a_CacLenVec.normalized;
        m_AddTimeCount += Time.deltaTime;

        if (m_MoveDurTime <= m_AddTimeCount)
        {
            m_isPickMvOnOff = false;
        }
        else
        {
            transform.position += m_MoveDir * Time.deltaTime * m_MoveVelocity;
            yasuo = YasuoState.trace;
        }

        if (m_TargetUnit != null)
        {
            m_isPickMvOnOff = true;
            a_CacTgVec = m_TargetUnit.transform.position - transform.position;
            if (a_CacTgVec.magnitude <= m_AttackDist && IsSkill == false)
                yasuo = YasuoState.attack;
        }
    }

    void ClearMsPickPath()
    {
        m_isPickMvOnOff = false;

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }

    void MousePick()
    {
        if (Input.GetMouseButtonDown(0) == false)
            return;

        if (GameMgr.IsPointerOverUIObject())
            return;

        if (yasuo == YasuoState.skill)
            return;

        a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value) == false)
            return;

        if (m_IsAttackCommandMode)
        {
            m_IsAttackCommandMode = false;
            HandleAttackCommandClick();
            return;
        }

        HandleNormalClick();
    }

    void HandleAttackCommandClick()
    {
        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
        {
            SetAttackTarget(hitInfo.collider.gameObject);

            if (GameMgr.Inst.m_CursorMark != null)
                GameMgr.Inst.m_CursorMark.SetActive(false);

            return;
        }

        GameObject nearEnemy = FindNearestEnemyInRange(hitInfo.point, m_AttackMoveSearchRadius);
        if (nearEnemy != null)
        {
            SetAttackTarget(nearEnemy);

            if (GameMgr.Inst.m_CursorMark != null)
                GameMgr.Inst.m_CursorMark.SetActive(false);
        }
        else
        {
            StopAutoAttack();
            MousePicking(hitInfo.point);
            GameMgr.Inst.CursorMarkOn(hitInfo.point);
        }
    }

    void HandleNormalClick()
    {
        StopAutoAttack();

        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
        {
            MousePicking(hitInfo.point, hitInfo.collider.gameObject);

            if (GameMgr.Inst.m_CursorMark != null)
                GameMgr.Inst.m_CursorMark.SetActive(false);
        }
        else
        {
            MousePicking(hitInfo.point);
            GameMgr.Inst.CursorMarkOn(hitInfo.point);
        }
    }

    // =========================
    // Rotation / Damage
    // =========================
    public void AttackRotUpdate()
    {
        if (m_TargetUnit == null)
            return;

        a_CacTgVec = m_TargetUnit.transform.position - transform.position;
        a_CacTgVec.y = 0.0f;

        float rotDist = Mathf.Max(m_AttackDist + 0.3f, m_AttackRange + 0.3f);
        if (a_CacTgVec.magnitude <= rotDist)
        {
            a_CacAtDir = a_CacTgVec.normalized;
            if (0.0001f < a_CacAtDir.magnitude)
            {
                a_CacRotSpeed = m_RotSpeed * 3.0f;
                Quaternion a_TargetRot = Quaternion.LookRotation(a_CacAtDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_CacRotSpeed);
            }
        }
    }

    public void TakeDamage(float a_Val)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Val;
        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        UpdateHpUI();

        if (m_CurHp <= 0.0f)
            PlayerDie();
    }

    void PlayerDie()
    {
        IsDie = true;
        AnimType("IsDie");
        colorCorrection.enabled = true;

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject monster in monsters)
        {
            monster.GetComponent<MonCtrl>().OnPlayerDie();
        }

        GameMgr.Inst.GameOver();
    }

    void AttDamage()
    {
        SwordCol.enabled = true;
    }

    void AttFinish()
    {
        SwordCol.enabled = false;
    }

    // =========================
    // Skill / Common helpers
    // =========================
    bool TryCancelMoveForSkill()
    {
        if (m_isPickMvOnOff == false)
            return false;

        transform.position += m_MoveDir * Time.deltaTime * m_MoveVelocity;
        yasuo = YasuoState.idle;
        ClearMsPickPath();
        return true;
    }

    void StopAutoAttackAndMove()
    {
        StopAutoAttack();
        TryCancelMoveForSkill();
    }

    bool IsSkillOnCooldown(float delay)
    {
        return delay > 0.0f;
    }

    void PlayTempEffect(GameObject effectPrefab, Vector3 pos, float destroyTime, out GameObject instance)
    {
        instance = Instantiate(effectPrefab, pos, Quaternion.identity);
        ParticleSystem ps = instance.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        Destroy(instance, destroyTime);
    }

    // =========================
    // Skills
    // =========================
    void UseSkill()
    {
        if (GameObject.FindGameObjectWithTag("Enemy") == null)
            return;

        if (Input.GetKeyDown(KeyCode.Q) == false)
            return;

        StopAutoAttackAndMove();

        if (IsSkillOnCooldown(skill_Delay))
        {
            ShowGuide("스킬 쿨타임 입니다.");
            return;
        }

        SwordCol.enabled = true;
        IsSkill = true;
        yasuo = YasuoState.skill;
        Aim.gameObject.SetActive(true);
        SkillEffect.SetActive(true);
        SkillEffect.GetComponent<ParticleSystem>().Play();
        StartCoroutine(Detecting());
    }

    void UseWSkill()
    {
        if (Input.GetKeyDown(KeyCode.W) == false)
            return;

        if (Ncnt <= 1)
        {
            ShowGuide("최소 2개의 다이아가 필요합니다.");
            return;
        }

        if (IsBuff)
        {
            ShowGuide("이미 적용 중 입니다.");
            return;
        }

        if (Wskill_Time > 0.0f)
        {
            ShowGuide("스킬 쿨타임 입니다.");
            return;
        }

        WDuration = 10.0f;
        IsBuff = true;
    }

    void UseFlash()
    {
        if (Input.GetKeyDown(KeyCode.F) == false)
            return;

        StopAutoAttackAndMove();

        if (IsSkillOnCooldown(Fskill_Delay))
        {
            ShowGuide("스킬 쿨타임 입니다.");
            return;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("MyTerrain")) == false)
            return;

        Vector3 effectpos = transform.position;
        effectpos.y += 1.5f;
        PlayTempEffect(FlashEffect, effectpos, 2.0f, out FlashInst);

        Vector3 dir = hit.point - transform.position;
        dir.y = 0.0f;
        dir.Normalize();

        float MaxMove = 8.0f;
        transform.position += dir * MaxMove;
        Fskill_Delay = Fskill_Time;
    }

    void UseHeal()
    {
        if (Input.GetKeyDown(KeyCode.D) == false)
            return;

        StopAutoAttackAndMove();

        if (IsSkillOnCooldown(Dskill_Delay))
        {
            ShowGuide("스킬 쿨타임 입니다.");
            return;
        }

        Vector3 effectpos = transform.position;
        effectpos.y += 1.5f;
        PlayTempEffect(HealEffect, effectpos, 2.0f, out HealInst);

        m_CurHp += 50.0f;
        if (m_CurHp > 100.0f)
            m_CurHp = 100.0f;

        UpdateHpUI();
        Dskill_Delay = Dskill_Time;
    }

    // =========================
    // Trigger / UI
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("attackpos"))
            TakeDamage(10);

        if (other.gameObject.name.Contains("CoinPrefab"))
        {
            Ncnt++;
            DiaCheck();
            Destroy(other.gameObject);
        }
    }

    void UiInfo()
    {
        GameMgr.Inst.SkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.WSkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.FSkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.DSkillCoolimg.gameObject.SetActive(true);

        skill_Delay -= Time.deltaTime;
        Dskill_Delay -= Time.deltaTime;
        Fskill_Delay -= Time.deltaTime;

        if (IsBuff)
        {
            WDuration -= Time.deltaTime;

            if (WDuration > 0.0f)
            {
                Trail.gameObject.SetActive(true);
                TrEff.GetComponent<TrailRenderer>().emitting = true;
                GameMgr.Inst.WSkillCoolimg.gameObject.SetActive(false);
                m_MoveVelocity = 20.0f;
                Skcnt = Ncnt;
                DiaCheck();
                GameMgr.Inst.WSkillInfoText.text = WDuration.ToString("N1");
            }

            if (WDuration <= 0.0f)
            {
                Trail.gameObject.SetActive(false);
                TrEff.GetComponent<TrailRenderer>().emitting = false;
                Ncnt = 1;
                Skcnt = Ncnt;
                DiaCheck();
                m_MoveVelocity = 8.0f;
                WDuration = 0.0f;
                Wskill_Time = 20.0f;
                IsBuff = false;
            }
        }
        else
        {
            Wskill_Time -= Time.deltaTime;
        }

        GameMgr.Inst.SkillCoolimg.fillAmount = skill_Delay / skill_Time;
        GameMgr.Inst.QSkillInfoText.text = skill_Delay.ToString("N1");

        if (Wskill_Time > 0.0f)
        {
            GameMgr.Inst.WSkillCoolimg.fillAmount = Wskill_Time / 30.0f;
            GameMgr.Inst.WSkillInfoText.text = Wskill_Time.ToString("N1");
        }

        GameMgr.Inst.FSkillCoolimg.fillAmount = Fskill_Delay / Fskill_Time;
        GameMgr.Inst.FSkillInfoText.text = Fskill_Delay.ToString("N1");

        GameMgr.Inst.DSkillCoolimg.fillAmount = Dskill_Delay / Dskill_Time;
        GameMgr.Inst.DSkillInfoText.text = Dskill_Delay.ToString("N1");

        if (skill_Delay <= 0.0f)
        {
            GameMgr.Inst.SkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.QSkillInfoText.text = "Q";
        }

        if (Wskill_Time <= 0.0f && WDuration <= 0.0f)
        {
            GameMgr.Inst.WSkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.WSkillInfoText.text = "W";
        }

        if (Dskill_Delay <= 0.0f)
        {
            GameMgr.Inst.DSkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.DSkillInfoText.text = "D";
        }

        if (Fskill_Delay <= 0.0f)
        {
            GameMgr.Inst.FSkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.FSkillInfoText.text = "F";
        }

        GuideTimer -= Time.deltaTime;
        if (GuideTimer <= 0.0f)
        {
            GameMgr.Inst.GuideText.gameObject.SetActive(false);
            GuideTimer = 0.0f;
            GameMgr.Inst.GuideText.text = "";
        }
    }

    // =========================
    // Q Detect
    // =========================
    IEnumerator Detecting()
    {
        cnt = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int nowCnt = 0;

        while (cnt > nowCnt)
        {
            while (AimF.fillAmount < 1)
            {
                a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                    {
                        Taget = hitInfo.transform;
                        AimF.fillAmount += (0.001f * Time.unscaledTime);

                        if (AimF.fillAmount >= 1f)
                        {
                            nowCnt++;
                            Taget.gameObject.layer = 0;
                            AimF.fillAmount = 0.0f;
                            yasuo = YasuoState.skilling;
                        }
                    }
                    else
                    {
                        AimF.fillAmount = 0.0f;
                    }
                }

                yield return null;

                if (Skcnt == nowCnt || nowCnt == cnt || Input.GetKeyDown(KeyCode.Q))
                {
                    if (yasuo != YasuoState.skill)
                        yield break;

                    yasuo = YasuoState.skillend;
                    yield break;
                }
            }

            yield return null;
        }
    }
}