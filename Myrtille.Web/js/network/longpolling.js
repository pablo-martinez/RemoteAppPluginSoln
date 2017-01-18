/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2016 Cedric Coste

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

/*****************************************************************************************************************************************************************************************************/
/*** Long-Polling                                                                                                                                                                                  ***/
/*****************************************************************************************************************************************************************************************************/

function LongPolling(config, dialog, display, network)
{
    var lpIFrame = null;

    this.init = function()
    {
        try
        {
            if (lpIFrame != null)
            {
                return;
            }

            //dialog.showDebug('creating long-polling iframe');

            lpIFrame = document.createElement('iframe');

            // IE < 9
            if (display.isIEBrowser() && display.getIEVersion() < 9)
            {
                lpIFrame.attachEvent('onload', this.reset);
            }
            // others
            else
            {
                lpIFrame.onload = this.reset;
            }
            
            lpIFrame.style.width = '0px';
            lpIFrame.style.height = '0px';
            lpIFrame.frameBorder = 0;
            
            document.body.appendChild(lpIFrame);
        }
	    catch (exc)
	    {
		    dialog.showDebug('long-polling init error: ' + exc.message);
            config.setLongPollingEnabled(false);
            lpIFrame = null;
	    }
    };

    this.reset = function()
    {
        if (config.getAdditionalLatency() > 0)
	    {
		    window.setTimeout(function() { load(); }, Math.round(config.getAdditionalLatency() / 2));
	    }
	    else
	    {
            load();
	    }
    };

    function load()
    {
	    try
	    {
            if (lpIFrame == null)
            {
                init();
                return;
            }

            //dialog.showDebug('loading long-polling iframe');

            lpIFrame.src = config.getHttpServerUrl() + 'PushUpdates.aspx' +
                '?sessionId=' + config.getHttpSessionId() +
                '&longPollingDuration=' + config.getLongPollingDuration() +
                '&imgIdx=' + display.getImgIdx() +
                '&noCache=' + new Date().getTime();
	    }
	    catch (exc)
	    {
		    dialog.showDebug('long-polling load error: ' + exc.message);
	    }
    }
}

/*****************************************************************************************************************************************************************************************************/
/*** External Calls                                                                                                                                                                                ***/
/*****************************************************************************************************************************************************************************************************/

var iframe = null;

function doIFrameCall(url)
{
    try
    {
        // browsers are limited to a given number of simultaneous xhrs or websockets calls; iframes are a nice hack to circumvent that (mainly useful on old browsers)
        iframe = document.createElement('iframe');
        iframe.src = url;

        document.body.appendChild(iframe);

        var display = new Display();

        // IE < 9
        if (display.isIEBrowser() && display.getIEVersion() < 9)
        {
            iframe.attachEvent('onload', function() { removeIframe(); });
        }
        // others
        else
        {
            iframe.onload = function() { removeIframe(); };
        }
    }
    catch (exc)
    {
        alert('doIFrameCall error: ' + exc.Message);
    }
}

function removeIframe()
{
    if (iframe != null)
    {
        document.body.removeChild(iframe);
    }
};