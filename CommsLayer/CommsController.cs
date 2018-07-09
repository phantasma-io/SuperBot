using System;
using DataLayer;
using MiddlemanLayer;

namespace CommsLayer
{
    public class CommsController
    {
        TelegramListener telegramListener;
        public void EnableComms()
        {
            //TODO: refactor so that we don't need to access the datalayer to retrieve settings. commslayer should never interact with the datalayer directly
            dynamic settings = DataUtils.ReadJsonToDynamic(DataUtils.Files.CommsSettings);
            telegramListener = new TelegramListener(settings);
        }
    }
}