# maldsChat

maldsChat is a chatbot powered by OpenAI and the Telegram Bot API. It uses OpenAI's language model to provide conversational responses and interacts with users on the Telegram platform.

## Description

maldsChat is built as an Azure Function using C# and integrates with the Telegram Bot API for communication with users. It leverages the capabilities of OpenAI's language model to generate human-like responses to user queries.

## Features

- Start a conversation with the chatbot using the `/start` command and receive a greeting message.
- Create a new conversation with the chatbot using the `/new` command.
- Interact with the chatbot by sending messages, and it will respond based on the conversation history.

## Setup

To use maldsChat, you need to set up the following:

1. **Telegram Bot Account**: Create a bot account on Telegram and obtain the API token.
2. **OpenAI API Key**: Obtain an API key from OpenAI to use their language model.
3. **Environment Variables**: Set the `telegramToken` and `openAiApiKey` environment variables with the respective values.

## Getting Started

1. Clone the repository: `git clone https://github.com/your-username/malds-chat.git`
2. Configure the required environment variables as mentioned in the Setup section.
3. Deploy the Azure Function to your Azure account.
4. Set up the webhook URL for your Telegram bot to point to the deployed Azure Function.
5. Start chatting with MaldsChat on Telegram!

## Support

If you have any questions, suggestions, or issues, please feel free to [open an issue](https://github.com/your-username/malds-chat/issues). We would be happy to assist you!

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
