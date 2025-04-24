
using System.Collections.Generic;

public static class RecipeMgr
{
    public static RecipesConfig GetRecipesConfig(string recipesId)
    {
        return ConfigManager.Instance.GetConfig<RecipesConfig>(recipesId);
    }

    public static IList<RecipesConfig> GetAllRecipesConfigs()
    {
        return ConfigManager.Instance.GetConfigList<RecipesConfig>();
    }

}