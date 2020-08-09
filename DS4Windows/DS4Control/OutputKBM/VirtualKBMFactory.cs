using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.DS4Control
{
    public static class VirtualKBMFactory
    {
        public static VirtualKBMBase DetermineHandler(string identifier =
            SendInputHandler.IDENTIFIER)
        {
            VirtualKBMBase handler = null;
            if (identifier == SendInputHandler.IDENTIFIER)
            {
                handler = new SendInputHandler();
            }
            else if (identifier == VMultiHandler.IDENTIFIER)
            {
                handler = new VMultiHandler();
            }
            else
            {
                handler = GetFallbackHandler();
            }

            return handler;
            //return new SendInputHandler();
            //return new VMultiHandler();
        }

        public static VirtualKBMMapping GetMappingInstance(string identifier =
            SendInputHandler.IDENTIFIER)
        {
            VirtualKBMMapping temp = null;
            if (identifier == SendInputHandler.IDENTIFIER)
            {
                temp = new SendInputMapping();
            }
            else if (identifier == VMultiHandler.IDENTIFIER)
            {
                temp = new VMultiMapping();
            }
            else
            {
                temp = GetFallbackMapping();
            }

            //VirtualKBMMapping temp = new VMultiMapping();
            //temp.PopulateConstants();
            return temp;
        }

        public static VirtualKBMBase GetFallbackHandler()
        {
            return new SendInputHandler();
        }

        public static VirtualKBMMapping GetFallbackMapping()
        {
            return new SendInputMapping();
        }

        /// <summary>
        /// Retrieves identifier string of fallback virtualkbm handler without
        /// creating an instance of the handler
        /// </summary>
        /// <returns>Identifier string of the default virtualkbm handler</returns>
        public static string GetFallbackHandlerIdentifier()
        {
            return SendInputHandler.IDENTIFIER;
        }

        public static bool IsValidHandler(string identifier)
        {
            bool result = false;
            switch (identifier)
            {
                case SendInputHandler.IDENTIFIER:
                    result = true;
                    break;
                case VMultiHandler.IDENTIFIER:
                    result = true;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
