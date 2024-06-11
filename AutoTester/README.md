### AutoUsabilityTester

#### How It Works?

1. Messages are now converted to tasks for an OpenAI model to process
2. The responses from the OpenAI model are converted to playwright operations and then executed
3. Each screenshot and OpenAI model return is sent to the front end to be displayed in the chat bot
4. New screenshots are sent to the OpenAI model in exchange for the next set of page interactions to feed into playwright
5. When the OpenAI model flags the task as completed, the websocket connection is idle until the user inputs a new task. 
 
