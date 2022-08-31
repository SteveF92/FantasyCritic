
namespace FantasyCritic.Lib.Extensions;
public static class NumberExtensions
{
    public static string ToDroppableString(this int droppable)
    {
        if (droppable == -1)
        {
            return "Unlimited";
        }

        return droppable.ToString();
    }

}
