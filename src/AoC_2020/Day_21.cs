using AoCHelper;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_21 : BaseDay
    {
        private readonly List<Food> _foods;

        public Day_21()
        {
            _foods = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            var ingredients = new HashSet<string>(_foods.SelectMany(food => food.Ingredients));
            var allergenIngredientList = SeparateAllergens(_foods);

            var safeIngredients = ingredients
                .Except(allergenIngredientList.Select(pair => pair.ingredient))
                .ToHashSet();

            return safeIngredients
                .Aggregate(0, (total, ingredient) =>
                    total + _foods.SelectMany(f => f.Ingredients).Count(ing => ing == ingredient))
                .ToString();
        }

        public override string Solve_2()
        {
            var allergenIngredientList = SeparateAllergens(_foods);

            return string.Join(",",
                allergenIngredientList.OrderBy(pair => pair.allergen).Select(pair => pair.ingredient));
        }

        private static HashSet<(string allergen, string ingredient)> SeparateAllergens(List<Food> foods)
        {
            var result = new HashSet<(string allergen, string ingredient)>();

            var allergens = new HashSet<string>(foods.SelectMany(food => food.Allergens));

            var allergensCandidateIngredients = new Dictionary<string, HashSet<string>>(
                allergens.Select(all => new KeyValuePair<string, HashSet<string>>(all, new HashSet<string>())));

            var foodsByAllergen = new Dictionary<string, HashSet<Food>>(
                allergens.Select(all => new KeyValuePair<string, HashSet<Food>>(all, new HashSet<Food>())));

            // Populate allergensCandidateIngredients and foodsByAllergen
            foreach (var food in foods)
            {
                foreach (var allergen in food.Allergens)
                {
                    foodsByAllergen[allergen].Add(food);
                    allergensCandidateIngredients[allergen].AddRange(food.Ingredients);
                }
            }

            // For each allergen:
            // Remove from candidates those ingredients that are not present in all foods that contain that allergen
            foreach (var pair in allergensCandidateIngredients)
            {
                foreach (var ingredient in pair.Value)
                {
                    if (!foodsByAllergen[pair.Key].All(food => food.Ingredients.Contains(ingredient)))
                    {
                        pair.Value.Remove(ingredient);
                    }
                }
            }

            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (var pair in allergensCandidateIngredients.Where(pair => pair.Value.Count == 1))
                {
                    var knownAllergen = pair.Key;
                    var knownIngredient = pair.Value.Single();

                    result.Add((allergen: knownAllergen, ingredient: knownIngredient));
                    allergensCandidateIngredients.Remove(knownAllergen);
                    allergensCandidateIngredients.ForEach(pair => pair.Value.Remove(knownIngredient));

                    changes = true;
                }
            }

            return result;
        }

        private IEnumerable<Food> ParseInput()
        {
            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                var split = line.Split("(", StringSplitOptions.TrimEntries);

                yield return new Food(
                    split[0].Split(" ", StringSplitOptions.TrimEntries),
                    split[1].TrimEnd(')').Replace("contains", "").Split(",", StringSplitOptions.TrimEntries));
            }
        }

        private class Food
        {
            public HashSet<string> Ingredients { get; init; }

            public HashSet<string> Allergens { get; init; }

            public Food(IEnumerable<string> ingredientes, IEnumerable<string> allergens)
            {
                Ingredients = new HashSet<string>(ingredientes);
                Allergens = new HashSet<string>(allergens);
            }
        }
    }
}
