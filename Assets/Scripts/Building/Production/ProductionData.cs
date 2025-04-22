using System;

[Serializable]
public class ProductionData
{
    public string instanceId;   // 实例ID
    public string recipeId;     // 配方ID

    // 所属生产平台实例id
    public string productionPlatformInstanceId;

    public int remainingTime;    // 剩余时间

    [NonSerialized]
    private RecipesConfig _recipe;    // 配方数据缓存

    public event Action<ProductionData> OnRecipeTimeChanged;
    public event Action<ProductionData> OnRecipeComplete;

    public ProductionData() { }

    public ProductionData(string recipeId, string productionPlatformInstanceId)
    {
        this.instanceId = System.Guid.NewGuid().ToString("N");
        this.recipeId = recipeId;
        this.productionPlatformInstanceId = productionPlatformInstanceId;

        _recipe = RecipeMgr.GetRecipesConfig(recipeId);
        if (_recipe != null)
        {
            remainingTime = _recipe.time;
        }
    }

    public RecipesConfig GetRecipe()
    {
        _recipe ??= RecipeMgr.GetRecipesConfig(recipeId);
        return _recipe;
    }

    public void SetTime(int time)
    {
        _recipe ??= RecipeMgr.GetRecipesConfig(recipeId);
        if (_recipe == null || _recipe.time <= 0)
            return;

        remainingTime = Math.Min(time, _recipe.time);
        OnRecipeTimeChanged?.Invoke(this);

        if (IsComplete())
        {
            OnRecipeComplete?.Invoke(this);
        }
    }

    public void ReduceTime(int deltaTime = 1)
    {
        SetTime(remainingTime - deltaTime);
    }

    public bool IsComplete()
    {
        _recipe ??= RecipeMgr.GetRecipesConfig(recipeId);

        return _recipe != null && _recipe.time > 0 && remainingTime <= 0;
    }
}
