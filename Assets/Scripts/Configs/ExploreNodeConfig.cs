public class ExploreNodeConfig : BaseConfig
{
    public string name;
    public string desc;
    public int type;
    public string ownMap;
    public bool isOnMap;
    public int[] mapLocation;
    public bool isStartPoint;
    public string[] neighborNodes;
    public string[] unlocksMidNodes;
    public string[] unlocksPostNodes;
    public string nodeAfterComplete;
    public string[] rewardIdGroup;
    public int[] rewardAmountGroup;
    public string[] requiredMaterialIdGroup;
    public int[] requiredMaterialAmountGroup;
    public double submitTime;
    public string[] recipeIdGroup;
    public string storyId;
    public string path;
}
