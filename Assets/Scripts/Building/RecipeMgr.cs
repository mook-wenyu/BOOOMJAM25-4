
public static class RecipeMgr
{
    public static RecipesConfig GetRecipesConfig(string recipesId)
    {
        return ConfigManager.Instance.GetConfig<RecipesConfig>(recipesId);
    }

    public static RecipeData StartRecipe(string recipeId, string buildingInstanceId)
    {
        var recipeData = new RecipeData(recipeId, buildingInstanceId);
        if (recipeData == null)
        {
            return null;
        }
        InventoryData playerInventory = InventoryMgr.GetPlayerInventoryData();
        bool hasEnoughMaterials = true;
        // 检查材料是否足够
        for (int i = 0; i < recipeData.GetRecipe().materialIDGroup.Length; i++)
        {
            if (!playerInventory.HasInventoryItem(recipeData.GetRecipe().materialIDGroup[i].ToString(), recipeData.GetRecipe().materialAmountGroup[i]))
            {
                hasEnoughMaterials = false;
                break;
            }
        }
        if (!hasEnoughMaterials)
        {
            return null;
        }
        // 消耗材料
        for (int i = 0; i < recipeData.GetRecipe().materialIDGroup.Length; i++)
        {
            playerInventory.RemoveInventoryItem(recipeData.GetRecipe().materialIDGroup[i].ToString(), recipeData.GetRecipe().materialAmountGroup[i]);
        }
        // 添加到正在生产的配方列表
        GameMgr.currentSaveData.recipeProgress.Add(recipeData);
        return recipeData;
    }
}