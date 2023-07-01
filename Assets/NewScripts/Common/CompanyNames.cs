using System.Collections.Generic;

namespace NewScripts.Common
{
    public static class CompanyNames
    {
        private static readonly System.Random Random = new System.Random();
        private static readonly string[] AllCompanyNames = {
            "Crunchy Bites",
            "Yumzy Delights",
            "Sizzlelicious",
            "Tasty Morsels",
            "Flavor Fusion",
            "Saucy Sizzlers",
            "NomNom Nibbles",
            "Munchy Munchkins",
            "Doughlicious",
            "Crispy Cravings",
            "Taste Explosion",
            "Yumster Foods",
            "Wholesome Delights",
            "Cheese 'n' Chomp",
            "Sweet Tooth Treats",
            "Spice It Up",
            "Yummy Tummy",
            "Mouthwatering Munchies",
            "Savory Bites",
            "Tummy Ticklers",
            "Craveable Cuisine",
            "Nacho Nation",
            "Fruit Fusion",
            "Snack Shack",
            "Gooey Goodies",
            "Choco Loco",
            "Sizzlin' Spuds",
            "Crispy Crunch",
            "Burger Bonanza",
            "Munchkin Meals",
            "Taco Fiesta",
            "Pizza Pleasers",
            "Finger Food Frenzy",
            "Whisked Wonders",
            "Juicy Jamboree",
            "Sippin' Smoothies",
            "Gourmet Grub",
            "Bread Basket",
            "Waffle Wonderland",
            "Tempting Treats",
            "Savory Soups",
            "Dippity Dips",
            "Pickle Paradise",
            "Hot Dog Haven",
            "Deli Delights",
            "Taste of Tuscany",
            "Sundae Supreme",
            "Noodle Nirvana",
            "Flapjack Fantasy",
            "Chewy Churros",
            "Slurp-a-licious",
            "Salsa Sensation",
            "Snack Attack",
            "Crunchy Croutons",
            "Berry Bliss",
            "Soda Pop Shop",
            "Chipper Chocolates",
            "Coco Loco",
            "Flavor Fiesta",
            "Butter Me Up",
            "Honey Heaven",
            "Pasta Paradise",
            "Whipped Delights",
            "Doughnut Dynasty",
            "Sweet 'n' Sour",
            "Spicy Sensations",
            "Taste of Tokyo",
            "Cheesy Delights",
            "Dumpling Delirium",
            "Sushi Central",
            "Burrito Bonanza",
            "Crunch Wrap Heaven",
            "Pretzel Paradise",
            "Cupcake Corner",
            "Popcorn Party",
            "Cookie Crusaders",
            "Ice Cream Insanity",
            "Smoothie Sensations",
            "Gelato Galore",
            "Scone City",
            "Wrap and Roll",
            "Spaghetti Squared",
            "Sausage Sensation",
            "Pepperoni Palace",
            "Burger Binge",
            "Sizzling Shakes",
            "French Fry Frenzy",
            "Choco Bliss",
            "Banana Bonanza",
            "Hot Sauce Heaven",
            "Chili Chuckles",
            "Fried Chicken Fiesta",
            "Sizzlin' Steaks",
            "Veggie Villa",
            "Berry Burst",
            "Caramel Craze",
            "Yogurt Yum-Yums",
            "Cheeseball Central",
            "Peanut Butter Paradise"
        };
        
        private static readonly List<string> AvailableNames = new(AllCompanyNames);
        
        public static string PickRandomName()
        {
            var index = Random.Next(0, AvailableNames.Count);
            string name = AvailableNames[index];
            AvailableNames.Remove(name);
            return name;
        }

        public static void AddToAvailableNames(string name)
        {
            AvailableNames.Add(name);
        }
    }
}