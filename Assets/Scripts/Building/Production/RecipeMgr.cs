
public static class RecipeMgr
{
    public static RecipesConfig GetRecipesConfig(string recipesId)
    {
        return ConfigManager.Instance.GetConfig<RecipesConfig>(recipesId);
    }
}