
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;

namespace Pizzo.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        List<string> pizza = new List<string>();
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(PizzaOptions);
            return Task.CompletedTask;
        }

        public async Task PizzaOptions(IDialogContext context, IAwaitable<object> result)
        {
            
            var message = context.MakeMessage();

            //setting the layout of the attachments to carousel type
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            //creating a list of message attachments which will be shown in carousel layout
            message.Attachments = new List<Attachment>();

            //creating hero cards showing pizza options
            var attachment1 = Cards.DynamicCardTemplates.getHeroCard("Cheese Margherita", "Rs. 99", "", "https://www.dominos.co.in//files/items/Double_Cheese_Margherita.jpg", new CardAction(ActionTypes.ImBack, "Add To Cart", value: "Adding Cheese Margherita"));
            var attachment2 = Cards.DynamicCardTemplates.getHeroCard("Farmhouse", "Rs. 139", "", "https://www.dominos.co.in//files/items/Farmhouse.jpg", new CardAction(ActionTypes.ImBack, "Add To Cart", value: "Adding Farmhouse"));

            //adding the created hero cards to the list of message attachments 
            message.Attachments.Add(attachment1);
            message.Attachments.Add(attachment2);

            //posting the hero cards carousel to the bot 
            await context.PostAsync(message);

            //function call to handle button click to add a pizza
            context.Wait(AddPizza);

        }

        public async Task AddPizza(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {   

            var response = await activity;
            var reply = context.MakeMessage();

            //add the pizza selected to the list
            if (response.Text.ToLower().Contains("cheese margherita")) 
                pizza.Add("cheese margherita");
         
            else if(response.Text.ToLower().Contains("farmhouse"))
                pizza.Add("farmhouse"); 

            //Prompt the user to enter number of pizza. Use PromptDialog to get the number 
            
            
            //ask the user if s/he wants to add more pizza
                PromptDialog.Text(
                    context: context,
                    resume: ResumeAddMorePizza,
                    prompt: "Do you want to continue adding more pizza? Last added: " + pizza.Last(),
                    retry: ""
                );
            }

        private async Task ResumeAddMorePizza(IDialogContext context, IAwaitable<string> activity)
        {   

            var response = await activity;

            //if the user wants to add more pizza, call the function to show the pizza options
            if (response.ToLower().Contains("yes")){

                await PizzaOptions(context, activity);
            }

            //if the user doesn't want to add more pizza, proceed further to show the cart contents
            else if(response.ToLower().Contains("no"))
            {
                var responseToNo = context.MakeMessage();
                
                var pizzaList = String.Join(", ", pizza.ToArray());
                responseToNo.Text = "Your cart contains: " + pizzaList;
                await context.PostAsync(responseToNo);
            }

            //handle any response other than "yes" or "no" and prompt the user to answer in "yes" or "no" 
            else
            {
                await context.PostAsync("Please answer in Yes or No");
            }
                
        }
    }
}

