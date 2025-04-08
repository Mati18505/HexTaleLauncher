import { useState } from "react";
import ReactDOM from 'react-dom/client'
import "./PopupMessage.css";
import {SettingsButton} from "./Menu/SettingsMenu";

function CloseBar({onClose}) {
    return <div className="close-bar">
        <span className="close-bar-nav" onClick={onClose}>&#10006;</span>
    </div>;
}

function MessageBox({message, onConfirm, onCancel})
{
    var [visible, SetVisible] = useState(true);

    var classes = visible? "" : "screen-overlay-hidden";

    return <>
    <div className={"screen-overlay "+classes}></div>
    <div className={"message-box"}>
        <CloseBar onClose={onCancel}/>
        <div className="message-box-text">{message}</div>
        <hr/>
        <div className="message-buttons-block">
            <SettingsButton label={"Continue"} onClick={onConfirm} enabled={true}/>
            <SettingsButton label={"Cancel"} onClick={onCancel} enabled={true}/>
        </div>
        
    </div>
    </>;
}


export default function Popup (message, onConfirm, onCancel) {

    const callback = (func) => {
        clear();
        if(typeof(func) !== 'undefined')
            func();
    };
    
    const PopupContent = () => {
        return (
            <MessageBox message={message} onConfirm={() => callback(onConfirm)} onCancel={() => callback(onCancel)}/>
        );
    };

    const clear = () => {
        root.unmount();
        node.remove();
    }
  
    const node = document.createElement("div");
    document.getElementById("app").appendChild(node);
    const root = ReactDOM.createRoot(node);
    root.render(<PopupContent/>);
};