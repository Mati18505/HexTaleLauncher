import React from "react";
import "./settingsMenuInputs.css"
import "./settingsMenu.css"

function Checkbox({name}) {
    return <label className="container">{name}
        <input type="checkbox" />
        <span className="checkmark"></span>
    </label>
}

function SettingsButton({label})
{
    return <button 
    className="settings-button" 
    type='button' 
    onClick={()=>{}}>
    <span>{label}</span>
    </button>;
}

function ListButton({label, onClick})
{
    return <div 
    className="settings-list-button"
    onClick={onClick}>
    <span>{label}</span>
    </div>;
}

function SaveBlock() {
    return <div className="save-block">
        <div className="save-buttons-block">
            <SettingsButton label="cancel"/>
            <SettingsButton label="save"/>
        </div>
    </div>;
}

export default function SettingsMenu() {
    return <div className="settings-container">
        <div>
            <ListButton 
            label={<><i className="icon-folder"></i> Show in folder</>}
            onClick={() => console.log("clicked folder")}
            />
            <ListButton 
            label={<><i className="icon-wrench"></i> Check and Repair</>}
            onClick={() => console.log("clicked repair")}
            />
            <ListButton 
            label={<><i className="icon-trash-empty"></i> Uninstall</>}
            />
            <hr/>
            <Checkbox name="Exit launcher when game starts"/>
        </div>
        <div>
            <SaveBlock/>
        </div>
    </div>;
}