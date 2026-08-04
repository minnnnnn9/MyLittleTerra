using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using MLT.Farm;
using MLT.Machine;
using MLT.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.Dev
{
    /// <summary>
    /// [테스트 전용] 컴포넌트 우클릭 → 각 시스템 테스트
    /// 연동 완료 후 삭제
    /// </summary>
    public class SystemTester : MonoBehaviour
    {
        [Header("농사 시스템")]
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Vector3Int _testCellPos = new Vector3Int(0, 0, 0);
        [SerializeField] private ItemSeedData _testSeed;
        [SerializeField] private FertilizerData _testFertilizer;

        [Header("씨앗 추출기")]
        [SerializeField] private SeedExtractor _seedExtractor;
        [SerializeField] private ItemCropData _testCrop;

        [Header("요리 시스템")]
        [SerializeField] private ItemBaseData _ingredient1;
        [SerializeField] private ItemBaseData _ingredient2;
        [SerializeField] private ItemBaseData _ingredient3;

        [Header("인벤토리")]
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private int _testSeedAmount = 10;

        [Header("플레이어")]
        [SerializeField] private PlayerController _playerController;

        // ───────────────────────────────────────────
        // 인벤토리
        // ───────────────────────────────────────────
        //[ContextMenu("테스트/인벤토리 - 씨앗 지급")]
        //public void GiveTestSeeds()
        //{
        //    if (!ValidateInventory() || !ValidateSeed()) return;
        //    bool result = _inventory.TryAddItem(_testSeed, _testSeedAmount);
        //    Debug.Log($"[테스트] {_testSeed._name} {_testSeedAmount}개 지급 결과: {result}");
        //}

        // ───────────────────────────────────────────
        // 농사 시스템(타일 변경)
        // ───────────────────────────────────────────
        //[ContextMenu("테스트/농사 - 1. 괭이질 (경작)")]
        //public void TestTill()
        //{
        //    if (!ValidateFarming()) return;
        //    ToolManager.Instance.SetTool(ToolType.HOE);
        //    ToolManager.Instance.ExecuteToolAction(_testCellPos);
        //    Debug.Log($"[테스트] 괭이질 실행 → 좌표: {_testCellPos}");
        //}

        //[ContextMenu("테스트/농사 - 2. 씨앗 심기")]
        //public void TestSow()
        //{
        //    if (!ValidateFarming() || !ValidateSeed()) return;
        //    ToolManager.Instance.SetSeed(_testSeed);
        //    ToolManager.Instance.ExecuteToolAction(_testCellPos);
        //    Debug.Log($"[테스트] 씨앗 심기 실행 → 좌표: {_testCellPos}, 씨앗: {_testSeed._name}");
        //}

        //[ContextMenu("테스트/농사 - 3. 물주기")]
        //public void TestWater()
        //{
        //    if (!ValidateFarming()) return;
        //    ToolManager.Instance.SetTool(ToolType.WATERING_CAN);
        //    ToolManager.Instance.ExecuteToolAction(_testCellPos);
        //    Debug.Log($"[테스트] 물주기 실행 → 좌표: {_testCellPos}");
        //}

        //[ContextMenu("테스트/농사 - 4. 수확 (낫)")]
        //public void TestHarvest()
        //{
        //    if (!ValidateFarming()) return;
        //    ToolManager.Instance.SetTool(ToolType.SCYTHE);
        //    ToolManager.Instance.ExecuteToolAction(_testCellPos);
        //    Debug.Log($"[테스트] 수확 실행 → 좌표: {_testCellPos}");
        //}

        //[ContextMenu("테스트/농사 - 5. 비료 주기")]
        //public void TestFertilize()
        //{
        //    if (!ValidateFarming()) return;
        //    if (_testFertilizer == null)
        //    {
        //        Debug.LogWarning("[테스트] _testFertilizer가 연결되지 않았습니다.");
        //        return;
        //    }
        //    ToolManager.Instance.SetFertilizer(_testFertilizer);
        //    ToolManager.Instance.ExecuteToolAction(_testCellPos);
        //    Debug.Log($"[테스트] 비료 주기 실행 → 좌표: {_testCellPos}");
        //}

        [ContextMenu("테스트/농사 - 6. 시간 100배속")]
        public void TestTimeSpeed()
        {
            if (TimeManager.Instance == null)
            {
                Debug.LogWarning("[테스트] TimeManager.Instance가 null입니다.");
                return;
            }
            TimeManager.Instance.SetTimeSpeed(100f);
            Debug.Log("[테스트] 시간 100배속 시작 → 날짜 넘어가면 자동으로 하루 경과 처리됨");
        }

        [ContextMenu("테스트/농사 - 7. 시간 정상속도")]
        public void TestTimeNormal()
        {
            if (TimeManager.Instance == null)
            {
                Debug.LogWarning("[테스트] TimeManager.Instance가 null입니다.");
                return;
            }
            TimeManager.Instance.SetTimeSpeed(1f);
            Debug.Log("[테스트] 시간 정상속도로 복구");
        }

        //// ───────────────────────────────────────────
        //// 씨앗 추출기
        //// ───────────────────────────────────────────
        //[ContextMenu("테스트/씨앗추출기 - 1. 작물 투입")]
        //private void TestInsertCrop()
        //{
        //    if (!ValidateExtractor()) return;
        //    if (_testCrop == null)
        //    {
        //        Debug.LogWarning("[테스트] _testCrop이 연결되지 않았습니다.");
        //        return;
        //    }
        //    bool result = _seedExtractor.InsertCrop(_testCrop);
        //    Debug.Log($"[테스트] 작물 투입 결과: {result}, 작물: {_testCrop._name}");
        //}

        //[ContextMenu("테스트/씨앗추출기 - 2. 상태 확인")]
        //private void TestExtractorStatus()
        //{
        //    if (!ValidateExtractor()) return;
        //    Debug.Log($"[테스트] 추출기 완료 여부: {_seedExtractor.IsReady()}");
        //}

        //[ContextMenu("테스트/씨앗추출기 - 3. 씨앗 수령")]
        //private void TestCollectSeed()
        //{
        //    if (!ValidateExtractor()) return;
        //    _seedExtractor.OnInteract(_playerController);
        //    Debug.Log("[테스트] 추출기 상호작용 실행");
        //}

        // ───────────────────────────────────────────
        // 요리 시스템
        // ───────────────────────────────────────────
        //[ContextMenu("테스트/요리 - 재료 지급")]
        //public void GiveIngredients()
        //{
        //    if (!ValidateInventory()) return;
        //    if (_ingredient1 == null || _ingredient2 == null || _ingredient3 == null)
        //    {
        //        Debug.LogWarning("[테스트] 재료(_ingredient1,2,3)가 모두 연결되지 않았습니다.");
        //        return;
        //    }

        //    bool r1 = _inventory.TryAddItem(_ingredient1, 1);
        //    bool r2 = _inventory.TryAddItem(_ingredient2, 1);
        //    bool r3 = _inventory.TryAddItem(_ingredient3, 1);
        //    Debug.Log($"[테스트] 재료 지급 결과 → {_ingredient1._name}:{r1}, {_ingredient2._name}:{r2}, {_ingredient3._name}:{r3}");
        //}

        //[ContextMenu("테스트/요리 - 요리 실행")]
        //public void TestCook()
        //{
        //    if (CookingManager.Instance == null)
        //    {
        //        Debug.LogWarning("[테스트] CookingManager.Instance가 null입니다.");
        //        return;
        //    }
        //    if (!ValidateInventory()) return;
        //    if (_ingredient1 == null || _ingredient2 == null || _ingredient3 == null)
        //    {
        //        Debug.LogWarning("[테스트] 재료(_ingredient1,2,3)가 모두 연결되지 않았습니다.");
        //        return;
        //    }

        //    // 재료 소지 확인
        //    if (!_inventory.CanRemoveItem(_ingredient1, 1) ||
        //        !_inventory.CanRemoveItem(_ingredient2, 1) ||
        //        !_inventory.CanRemoveItem(_ingredient3, 1))
        //    {
        //        Debug.LogWarning("[테스트] 재료가 부족합니다. 먼저 '재료 지급'을 실행하세요.");
        //        return;
        //    }

        //    // 재료 차감
        //    _inventory.TryRemoveItem(_ingredient1, 1);
        //    _inventory.TryRemoveItem(_ingredient2, 1);
        //    _inventory.TryRemoveItem(_ingredient3, 1);

        //    // 요리 실행
        //    var ingredients = new ItemBaseData[] { _ingredient1, _ingredient2, _ingredient3 };
            //var result = CookingManager.Instance.Cook(ingredients);
            //Debug.Log($"[테스트] 요리 결과: {(result.IsSuccess ? "성공" : "실패")}, 아이템: {result.ResultItem?._name}, 수량: {result.Amount}");
        //}

        // ───────────────────────────────────────────
        // 유효성 검사
        // ───────────────────────────────────────────
        //private bool ValidateFarming()
        //{
        //    if (ToolManager.Instance == null)
        //    {
        //        Debug.LogWarning("[테스트] ToolManager.Instance가 null입니다.");
        //        return false;
        //    }
        //    if (_tilemap == null)
        //    {
        //        Debug.LogWarning("[테스트] _tilemap이 연결되지 않았습니다.");
        //        return false;
        //    }
        //    return true;
        //}

        //private bool ValidateSeed()
        //{
        //    if (_testSeed == null)
        //    {
        //        Debug.LogWarning("[테스트] _testSeed가 연결되지 않았습니다.");
        //        return false;
        //    }
        //    return true;
        //}

        //private bool ValidateInventory()
        //{
        //    if (_inventory == null)
        //    {
        //        Debug.LogWarning("[테스트] _inventory가 연결되지 않았습니다.");
        //        return false;
        //    }
        //    return true;
        //}

        //private bool ValidateExtractor()
        //{
        //    if (_seedExtractor == null)
        //    {
        //        Debug.LogWarning("[테스트] _seedExtractor가 연결되지 않았습니다.");
        //        return false;
        //    }
        //    return true;
        //}

        public void TestTimeSpeed10()
        {
            if (TimeManager.Instance == null)
            {
                Debug.LogWarning("[테스트] TimeManager.Instance가 null입니다.");
                return;
            }
            TimeManager.Instance.SetTimeSpeed(10f);
            Debug.Log("[테스트] 시간 10배속 시작 → 날짜 넘어가면 자동으로 하루 경과 처리됨");
        }



    }
}