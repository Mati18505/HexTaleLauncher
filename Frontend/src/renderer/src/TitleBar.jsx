import React, { useState } from 'react';
import './TitleBar.css';

function Minimize()
{
    app.window.minimize();
}

function CloseApp() 
{
    app.window.close();
}

export default function TitleBar()
{
    var [menu, ShowMenu] = useState(false);

    return <div id="TitleBar">
        <div className="left">
            <div className="titleParent"><span className="title">HexTale Launcher</span></div>
        </div>
        <div className="right">
            <span className='navLink' onClick={Minimize}>&minus;</span>
            <span className='navLink' onClick={CloseApp}>&#10006;</span>
        </div>
        
    </div>;
}