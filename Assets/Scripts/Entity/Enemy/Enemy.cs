using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : LivingEntity
{
    private static List<Enemy> _enemies = new();

    [SerializeField]
    public Vector2Int Size = new Vector2Int(1, 1);
    [SerializeField]
    public float FollowDistance = 40f;

    private Vector3 _beforePos;
    private float _moveCheckTimer = 0f;
    protected GameObject lifeBack;
    protected Image lifeBarImg;
    private GameObject _icon;
    private float _pathFindTimer = 0f;
    private Stack<Vector3> _paths = new();
    private Vector3 _beforePath;

    [HideInInspector]
    public int DropGoldAmount = 0; 
    
    public static Enemy SpawnEnemy(string name, Vector2 pos)
    {
        return SpawnEnemy(Resources.Load<GameObject>("Enemies/" + name), pos);
    }
    
    public static Enemy SpawnEnemy(Enemy enemy, Vector2 pos)
    {
        return SpawnEnemy(enemy.gameObject, pos);
    }
    
    public static Enemy SpawnEnemy(GameObject enemy, Vector2 pos)
    {
        var obj = Instantiate(enemy, Storage.Get("EnemyContainer").transform);
        obj.transform.position = pos;
        ParticleManager.Instance.SpawnParticle(pos, ParticleType.Smoke, 0.5f, 0, 10);
        var e = obj.GetComponent<Enemy>();
        return e;
    }

    public static IEnumerable<Enemy> GetEnemies()
    {
        foreach(var e in _enemies) yield return e;
    }

    protected override void Awake()
    {
        _enemies.Add(this);
        base.Awake();

        _beforePos = transform.position;
        lifeBack = Instantiate(Resources.Load<GameObject>("Enemies/EnemyBar"), spriteRenderer.transform);
        lifeBarImg = lifeBack.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        _beforePath = transform.position;
        _icon = Instantiate(Resources.Load<GameObject>("Enemies/EnemyIcon"), transform);
    }

    protected void ResizeHpBar()
    {
        var s = lifeBack.transform.localScale;
        var ps = transform.localScale;
        lifeBack.transform.localScale = new Vector3(s.x / ps.x, s.y / ps.y);
    }

    protected void ResizeIcon()
    {
        var s = _icon.transform.localScale;
        var ps = transform.localScale;
        _icon.transform.localScale = new Vector3(s.x / ps.x, s.y / ps.y);
    }

    private void OnDestroy() {
        _enemies.Remove(this);
    }

    protected override void Start()
    {
        base.Start();
        ResizeHpBar();
        ResizeIcon();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        DisplayUpdate();
    }

    protected void DisplayUpdate()
    {
        lifeBarImg.fillAmount = Life / Attribute.GetValue(AttributeType.MaxLife);
    }

    protected override void Move()
    {
        if (axis.magnitude > 0f && (_moveCheckTimer -= Time.deltaTime) < 0f)
        {
            _moveCheckTimer += 0.1f;
            if ((_beforePos - transform.position).magnitude == 0)
            {
                var p = transform.position;
                transform.position = new Vector3(Mathf.RoundToInt(p.x * 2) * 0.5f, Mathf.RoundToInt(p.y * 2) * 0.5f, p.z);
            }
            _beforePos = transform.position;
        }
        base.Move();
    }

    protected override IEnumerator KillEffect()
    {
        yield return base.KillEffect();
        _enemies.Remove(this);
        var sqrtValue = (int)(Mathf.Sqrt(DropGoldAmount));
        while(DropGoldAmount > 0)
        {
            var amount = Mathf.Clamp(Random.Range(sqrtValue, (int)(sqrtValue * 1.5)), 1, DropGoldAmount);
            DropGoldAmount -= amount;
            Gold.SpawnGold(transform.position, amount);
        }
        Destroy(gameObject);
    }

    protected void FollowPlayer()
    {
        if(Player.Instance.IsDead) return;

        // 몸의 사이즈를 포함해서 일직선상으로 플레이어가 시야에 잡힌다면, 플레이어 방향으로 이동
        var playerDir = (Player.Instance.transform.position - transform.position).normalized;
        var hit = Physics2D.CircleCast(transform.position, Mathf.Max(Size.x, Size.y), playerDir,
                FollowDistance, ~LayerMask.GetMask("Ignore Raycast"));
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
        {
            axis = playerDir;
            return;
        }

        // 플레이어가 탐지 반경 내에 있다면 일정 시간마다 a* 알고리즘으로 길찾기
        if (Vector2.Distance(transform.position, Player.Instance.transform.position) < FollowDistance
                && (_pathFindTimer -= Time.deltaTime) < 0f)
        {
            _pathFindTimer += .3f;
            Vector3[] endPoints = new[] { Vector3.zero, Vector3.down * Size.y, Vector3.up * Size.y, 
                    Vector3.left * Size.x, Vector3.right * Size.x };
            var len = -1;
            foreach(var end in endPoints) {
                len = PathFinder.Instance.GetPath(_paths, transform.position, Player.Instance.transform.position + end, Size);
                if(len >= 0) break;
            }
            //경로를 찾을 수 없거나 경로의 총 길이가 탐기 반경보다 길다면 5초마다 경로탐색 / 기존 경로 삭제
            if(len == -1 || len > FollowDistance) 
            {   
                _paths.Clear();
                _pathFindTimer = 5f;
            }
        }

        // 경로가 있다면, 따라가고, 없다면 플레이어 방향으로 이동
        if (_paths.Count > 0)
        {
            if (Vector2.Distance(_beforePath, _paths.Peek()) > Vector2.Distance(transform.position, _beforePath))
            {
                axis = (_paths.Peek() - transform.position).normalized;
                if (axis.magnitude == 0)
                {
                    NextPath();
                    transform.position = _beforePath;
                }
            }
            else NextPath();
        }
        else if(Vector2.Distance(transform.position, Player.Instance.transform.position) < FollowDistance)
        {
            axis = (Player.Instance.transform.position - transform.position).normalized;
        }
    }

    private void NextPath()
    {
        _beforePath = _paths.Pop();
        if (_paths.Count > 0) axis = (_paths.Peek() - transform.position).normalized;
    }
}
