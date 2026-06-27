using UnityEngine;
using NCMS.Utils;

namespace ModernBox
{
    public class PizzaManager : MonoBehaviour
    {
        public static PizzaManager instance;

        private int pizzaClicks = 0;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        public void ClickPizza()
        {
            pizzaClicks++;

            switch (pizzaClicks)
            {
                case 1:
                    WorldTip.showNow("The first pizza click... the journey begins.", true, "top", 3f);
                    break;
                case 10:
                    WorldTip.showNow("10 pizza clicks! You're just getting started.", true, "top", 3f);
                    break;
                case 20:
                    WorldTip.showNow("20 clicks! That's a lot of finger grease.", true, "top", 3f);
                    break;
                case 30:
                    WorldTip.showNow("30 clicks! This pizza is feeling the pressure.", true, "top", 3f);
                    break;
                case 40:
                    WorldTip.showNow("40 clicks! Are you... okay?", true, "top", 3f);
                    break;
                case 50:
                    WorldTip.showNow("Halfway to pizza insanity. 50 clicks!", true, "top", 3f);
                    break;
                case 60:
                    WorldTip.showNow("60 clicks. This is beyond casual pizza behavior.", true, "top", 3f);
                    break;
                case 70:
                    WorldTip.showNow("70 clicks... pizza is starting to sweat.", true, "top", 3f);
                    break;
                case 80:
                    WorldTip.showNow("80 clicks! Your dedication to pizza is admirable.", true, "top", 3f);
                    break;
                case 90:
                    WorldTip.showNow("90 clicks! Final stretch to the ultimate pizza moment.", true, "top", 3f);
                    break;
                case 100:
                    WorldTip.showNow("100 clicks! YOU HAVE ENTERED... PIZZA MODE!", true, "top", 3f);
                    ActivatePizzaMode();
                    break;
            }
        }

        private void ActivatePizzaMode()
        {
                    new TabBuilder()
                    .SetTabID("PizzaTab")
                    .SetName("Pizza")
                    .SetDescription("Yeah uh, can I get a large... no, extra large pizza... but I want half of it deep-dish and the other half thin crust... but like, swirl 'em together? Also I want triple cheese, but only vegan cheese on the left side, and blue cheese drizzle on the right. Toppings? Yeah okay: I need double anchovies, pineapple rings stacked like Saturn’s rings, crushed jalapeño poppers, fried pickles, and six uncut mozzarella sticks buried under the sauce like treasure. And before you bake it, I want you to spell 'PIZZA GOD' in pepperoni. Also, crust has to be stuffed—with peanut butter. No substitutions. Oh—and can you lightly dust the whole thing with powdered sugar? My cousins on a weird diet. And put it in a box shaped like a hexagon. Thanks.")
                    .SetPosition(200)
                    .SetToolbarButtonVisible(false)
                    .SetRepeatPressReturnTab("ModernBoxTab")
                    .SetIcon("ui/icons/Pizza")
                    .Build();
                    
                    Windows.ShowWindow("PizzaWindow");

                GameObject simulatorGO = new GameObject("PizzaSimulator");
                simulatorGO.AddComponent<PizzaSimulator>();
                DontDestroyOnLoad(simulatorGO);
        }
    }
}