namespace MLT.Core
{
    public enum ToolType
    {
        NONE,
        AXE,
        PICKAXE,
        HAND,
        HOE,
        WATERING_CAN,
        SCYTHE,
        SEED,
        FERTILIZER,
        BOW
    }

    public enum ToolGrade
    {
        NONE = 0,
        COPPER = 1,
        IRON = 2,
        GOLD = 3,
        IRIDIUM = 4
    }

    public enum FarmState
    {
        EMPTY,          
        TILLED,         
        SEEDED,         
        GROWING,        
        HARVESTABLE,   
    }

    public enum Season
    {
        SPRING, SUMMER, FALL, WINTER
    }

    // ���ɻ�
    public enum Interest
    {
        FARM, WEATHER, SPORTS, FASHION, ANIMAL, FLOWER, MINERAL, FOOD, FISH
    }
    public enum ItemType
    {
        NONE, CROP, TOOL, WATERINGCAN, FORAGE, SEED, MATERIAL, FOOD
    }

    public enum FertilizerType
    {
        None,
        GrowthBooster,  
        MoistSoil       
    }


    public enum MiningActionType
    {
        FAIL,
        INSTANTSUCCESS,
        MINIGAME
    }

    // NPC 타입
    public enum NPCType
    {
        HUMAN, ANIMAL
    }

    // 성별
    public enum Gender
    {
        MALE, FEMALE
    }

    // 행동 타입
    public enum ActionType
    {
        SLEEP, BEDTIME, EAT, MEALTIME, DRINK, SHOWER, PLAY, WORK, IDLE, TALK
    }

    // NCP GOAP WolrdState에 사용될 NPC 현재 상태
    public enum NPCStateKey
    {
        ATTARGET,   // 목적지에 도착 하였는가?
        ISSLEEPY,   // 졸린가?
        ISHUNGRY,   // 배고픈가?
        ISWORKING,  // 일하는 중인가?
        ISTHIRSTY,  // 목이 마른가?
        ISDIRTY,    // 더러운가?
        ISBORING,   // 지루한가?
        ISBEDTIME,  // 잠을 잘 시간인가?
        ISMEALTIME, // 식사 시간인가?
        ISWORKTIME, // 근무 시간인가?
        ISIDLING,   // 할 일이 없는가? (대기 상태)
        ISLONELY    // 외로운가? (사교욕구)
    }

    public enum TabType
    {
        INVENTORY,
        RELATIONSHIP,
        CRAFTING,
        BUNDLE,
        EQUIPMENT,
        QUEST,
        GAMESETTING,
        NONE
    }

    // 말 성장단계
    public enum HorseLifeStage
    {
        JUVENILE, ADULT
    }

    // 말 주법 
    public enum HorseRunningStyle
    {
        FRONT,      // 도주
        PACESETTER, // 선행
        STALKER,    // 선입
        CLOSER      // 추입
    }

    public enum RacingSegmentType
    {
        EARLY,
        MIDDLE,
        CLOSING,
        FINALSTRETCH
    }

    public enum RacingCourseType
    {
        STRAIGHT,
        CORNER,
        UPHILL,
        DOWNHILL,
        FINALSTRAIGHT
    }

    public enum RaceGrade
    {
        BRONZE, SILVER, GOLD
    }

    public enum RacingEventType 
    { 
        SPEED_BUFF, 
        SPEED_DEBUFF, 
        STAMINA_HEAL,
        STAMINA_DOWN
    }

    public enum TalkType
    {
        NONE, COMMON, CURRENT_ACTION, CHAT, GAME_TIP, 
        GOOD_REACTION, NORMAL_REACTION, BAD_REACTION, 
        QUEST, QUEST_CHECK, QUEST_CLEAR, QUEST_FAILED,
        DEBT_REPAY, DEBT_REMAIN, DEBT_REPAY_ALL, DEBT_REPAY_FAILED
    }

    public enum SFXType
    {
        ITEM_PICKUP,
        SEED_PLANT,
        WATERING,
        TILL_SOIL,
        ARTIFACT_DIG,
        UI_OPEN,
        UI_CLOSE,
        DIALOGUE_OPEN,
        BUY_SUCCESS,
        BUY_FAIL,
        BREAK_PLACE,
        MACHINE_INSERT,
        HARVEST,
        TELEPORT,
        CROW_CRY,
        CROW_CRY_FAST,
        ITEM_PLACE,
        NPC_MAN_SAY
    }
}