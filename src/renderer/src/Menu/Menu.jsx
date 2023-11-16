import React from "react";
import SettingsMenu from "./SettingsMenu";
import "./Menu.css";

export default function Menu({menu, onHide}) {

    const availableMenus = ["settings"];

    var visible = availableMenus.includes(menu);

    var classes = "menu ";
    if(!visible)
        classes+="hidden";

    var content = "";

    if(menu == "settings")
        content = <SettingsMenu />

    return <div className={classes}>
        <h1 className="menu-header">{menu}</h1>
        <div className="menu-content">
            {content}
        </div>
    </div>;
}