using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SavedGamesView : MonoBehaviour {

    public List<string> strings = new();

}

public static class Pagination {
    public static int PagesCount(this int itemsCount, int itemsPerPage) {
        return (itemsCount + itemsPerPage - 1) / itemsPerPage;
    }
    public static int PagesCount<T>(this IReadOnlyList<T> list, int itemsPerPage) {
        return PagesCount(list.Count, itemsPerPage);
    }
    public static (int low, int high) PagesSlice(int pagesCount, int current, int radius) {
        return MathUtils.FitSegment(pagesCount, current - radius, current + radius);
    }
    public static IEnumerable<T> GetPage<T>(this IEnumerable<T> items, int itemsPerPage, int page) {
        return items.Skip(itemsPerPage * page).Take(itemsPerPage);
    }
}