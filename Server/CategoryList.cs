public class CategoryList
{
    private List<Category> categories = new List<Category>
    {
        new Category { Cid = 1, Name = "Beverages" },
        new Category { Cid = 2, Name = "Condiments" },
        new Category { Cid = 3, Name = "Confections" }
    };

    public Category? CreateCategory(string categoryName)
    {
        if (categories.Exists(c => c.Name == categoryName))
        {

            return null;
        }

        Category newCategory = new Category { Cid = categories.Count + 1, Name = categoryName };
        categories.Add(newCategory);
        return newCategory;

    }

    public Category? ReadCategory(int cid)
    {
        var category = categories.Find(c => c.Cid == cid);
        if (category == null)
        {
            return null;
        }
        return category;
    }

    // Update an existing category
    public void UpdateCategory(Category category)
    {
        var index = categories.FindIndex(c => c.Cid == category.Cid);
        if (index != -1)
        {
            categories[index] = category;
        }
        else
        {
            Console.WriteLine($"Category with ID {category.Cid} not found.");
        }
    }

    public Category? DeleteCategory(int cid)
    {
        var category = categories.Find(c => c.Cid == cid);
        if (category != null)
        {
            categories.Remove(category);
            return category;
        }
        else
        {
            return null;
        }
    }

    public Category[] ListCategories()
    {
        if (categories.Count == 0)
        {
            Console.WriteLine("No categories available.");
            return [];
        }
        else
        {
            return [.. categories];
        }
    }
}