using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class FormationEditor : MonoBehaviour
{
    [SerializeField]
    private int _mapSize;
    [Space] [Space] [Space]
    [SerializeField]
    private Formation _result;
    [Space] [Space] [Space]
    [SerializeField]
    private RectTransform _editorContents;
    [SerializeField]
    private RectTransform _editorScrollView, _blockContents;
    [SerializeField]
    private GameObject _blockPrefab;
    [SerializeField]
    private Sprite _emptyBox;
    [SerializeField]
    private Image[] _pivots;
    [SerializeField]
    private GameObject _enemyNotice;
    [SerializeField]
    private TextMeshProUGUI _minEnemyLevel, _maxEnemyLevel;

    private float _blockSize;
    private BlockButton[] _blocks;
    private Dictionary<Vector2Int, Image> _editorBlockMap = new(); 
    private int _curBlockIdx = 0;
    private Pivot _curPivot = Pivot.Center;
    private EnemyPos _holdEnemyPos;

    private void Awake() {

        List<BlockButton> blockList = new();
        foreach(var obj in Resources.LoadAll<GameObject>("Blocks/"))
        {
            var block = obj.GetComponent<Block>();
            if(block == null) continue;
            blockList.Add(new(block));
        }
        _blocks = blockList.ToArray();

        for(var i = 0; i < _blocks.Length; i++)
        {
            var blockIdx = i;
            var block = Instantiate(_blockPrefab, _blockContents.transform);
            var blockImage = block.GetComponent<Image>();
            var blockButton = block.GetComponent<Button>();

            _blocks[i].BlockImage = blockImage;

            blockImage.sprite = _blocks[i].Block.gameObject.GetComponent<SpriteRenderer>().sprite;
            blockButton.onClick.AddListener(() => {
                _curBlockIdx = _curBlockIdx == blockIdx ? -1 : blockIdx;
            });
        }

        var layout = _editorContents.GetComponent<GridLayoutGroup>();
        _blockSize = _editorScrollView.sizeDelta.x / _mapSize;

        var start = Vector2Int.zero;
        var end = Vector2Int.one * (_mapSize - 1);

        for(var y = _mapSize - 1; y >= 0; y--)
        {
            for(var x = 0; x < _mapSize; x++)
            {   
                var pos = new Vector2Int(x, y);
                var block = Instantiate(_blockPrefab, _editorContents.transform);
                var blockImage = block.GetComponent<Image>();
                var blockButton = block.GetComponent<Button>();

                _editorBlockMap[pos] = blockImage;

                layout.cellSize = Vector2.one * _blockSize;
                FillEmpty(blockImage, pos);
                blockButton.onClick.AddListener(() => {
                    var pivotPos = pos - Formation.GetPivot(start, end, _curPivot);
                    
                    _result.Blocks.Remove(_result.Blocks.Find(blockPos => 
                            Formation.GetPivot(start, end, blockPos.Pivot) + blockPos.Pos == pos));
                    _result.Enemies.Remove(_result.Enemies.Find(enemyPos => 
                            Formation.GetPivot(start, end, enemyPos.Pivot) + enemyPos.Pos == pos));

                    if(_holdEnemyPos != null)
                    {
                        _holdEnemyPos.Pivot = _curPivot;
                        _holdEnemyPos.Pos = pivotPos;
                        _result.Enemies.Add(_holdEnemyPos);
                        _holdEnemyPos = null;
                        return;
                    }

                    if(_curBlockIdx == -1) return;
                    
                    BlockPos newBlockPos = new();
                    newBlockPos.Pivot = _curPivot;
                    newBlockPos.Pos = pivotPos;
                    newBlockPos.Block = _blocks[_curBlockIdx].Block;
                    _result.Blocks.Add(newBlockPos);
                });
            }
        }
    }

    private void Update() {

        var start = Vector2Int.zero;
        var end = Vector2Int.one * (_mapSize - 1);

        foreach(var pivotImg in _pivots)
        {
            pivotImg.color = Color.white;
        }
        _pivots[(int)_curPivot].color = Color.cyan;

        for(var i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].BlockImage.color = i == _curBlockIdx ? Color.white : new Color(1, 1, 1, 0.5f);
        }

        for(var y = 0; y < _mapSize; y++)
        {
            for(var x = 0; x < _mapSize; x++)
            {   
                var pos = new Vector2Int(x, y);
                var blockImage = _editorBlockMap[pos];
                FillEmpty(blockImage, pos);
            }
        }

        foreach(var blockPos in _result.Blocks)
        {
            var blockImage = _editorBlockMap[Formation.GetPivot(start, end, blockPos.Pivot) + blockPos.Pos]; 
            blockImage.sprite = blockPos.Block.gameObject.GetComponent<SpriteRenderer>().sprite;
            blockImage.color = Color.white;
        }

        foreach(var enemyPos in _result.Enemies)
        {
            var blockImage = _editorBlockMap[Formation.GetPivot(start, end, enemyPos.Pivot) + Vector2Int.FloorToInt(enemyPos.Pos)]; 
            blockImage.sprite = _emptyBox;
            blockImage.color = Color.Lerp(Color.yellow, Color.red, (enemyPos.EnemyIdxMax + enemyPos.EnemyIdxMin) * 0.5f / 4f);
        }

        _enemyNotice.SetActive(_holdEnemyPos != null);

    }

    private void FillEmpty(Image blockImage, Vector2Int pos)
    {
        blockImage.sprite = _emptyBox;
        blockImage.color = new Color(1, .8f, 1, (pos.x + pos.y) % 2 == 0 ? .3f : .1f);
    }

    public void SetCurrentPivot(int pivot)
    {
        _curPivot = (Pivot)pivot;
    }

    public void PrepareEnemy()
    {
        _holdEnemyPos = new();
        if(int.TryParse(Regex.Replace(_minEnemyLevel.text, "[^0-9]", ""), out int minLv)) _holdEnemyPos.EnemyIdxMin = minLv;
        if(int.TryParse(Regex.Replace(_maxEnemyLevel.text, "[^0-9]", ""), out int maxLv)) _holdEnemyPos.EnemyIdxMax = maxLv;
    }
}

public class BlockButton
{
    public Block Block;
    public Image BlockImage;

    public BlockButton(Block block)
    {
        Block = block;
    }
}