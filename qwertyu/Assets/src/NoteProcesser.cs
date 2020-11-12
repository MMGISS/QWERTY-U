﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteProcesser : MonoBehaviour {
    
    public char type;
    public char lane;
    public sbyte keyNum;
    public AnimationCurve[] curve;
    public long hitTick;
    public long appearPosition;
    public long hitPosition;
    public short longNoteID;
    public bool isReversed;
    public bool isMultiNote;
    public float positionNotesFrom;

    float appearAlpha = 0;
    
    MaterialPropertyBlock mpb;
    SpriteRenderer timingSupportSR;
    SpriteRenderer noteSR;
    LaneProcesser parentSrcComp;

    void Start() {
        mpb = new MaterialPropertyBlock();
        timingSupportSR = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        if (type == '1' || type == '2') noteSR = gameObject.GetComponent<SpriteRenderer>();
        parentSrcComp = transform.parent.gameObject.GetComponent<LaneProcesser>();

        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    void Update() {
        
        float time = (float)(parentSrcComp.position - appearPosition) / (hitPosition - appearPosition);
        
        if (time < 0) {
            transform.localPosition = new Vector3(0, isReversed ? -positionNotesFrom : positionNotesFrom, 0);
            appearAlpha = 0;
        } else if (time > 1) {
            transform.localPosition = new Vector3(0, 0, 0);
            if (longNoteID == -1 || GameMaster.longNoteInfoStorage[longNoteID].endTime > 1) appearAlpha = 0;
        } else {
            for (int i = curve.Length - 1; i > 0; i--) time = 1 - curve[i].Evaluate(time);
            transform.localPosition = new Vector3(0f, isReversed ? curve[0].Evaluate(time) * -positionNotesFrom : curve[0].Evaluate(time) * positionNotesFrom, 0);
            if (type == '1' || type == '2' || GameMaster.longNoteInfoStorage[longNoteID].startTime > 1) appearAlpha += (1 - appearAlpha) / 10;
        }
        
        float scale = 1 - (float)Math.Pow(1 - (hitTick - GameMaster.gameMasterTime) / 30000000f, 10);
        float alpha = time < 0 || time > 1 ? 0 : Math.Min(Math.Max(1 - (hitTick - GameMaster.gameMasterTime) / 30000000f, 0), 1);

        if (type == '1' || type == '2') {
            if (longNoteID != -1) {
                GameMaster.longNoteInfoStorage[longNoteID].startPosition = transform.localPosition.y;
                GameMaster.longNoteInfoStorage[longNoteID].startTime = time;
                GameMaster.longNoteInfoStorage[longNoteID].alpha = (byte)(80 * appearAlpha);
            }

            transform.GetChild(0).localScale = new Vector3(scale * 1.5f + 1, scale * 1.5f + 1, 1);
            mpb.SetColor("_Color", new Color(1, 1, 1, alpha * appearAlpha));
            timingSupportSR.SetPropertyBlock(mpb);

            if ((time < 0 || time > 1) && (longNoteID == -1 || GameMaster.longNoteInfoStorage[longNoteID].endTime > 1)) mpb.SetColor("_Color", new Color(1, 1, 1, 0));
            else mpb.SetColor("_Color", new Color(1, 1, 1, appearAlpha));
            noteSR.SetPropertyBlock(mpb);
        } else {
            GameMaster.longNoteInfoStorage[longNoteID].endPosition = transform.localPosition.y;
            GameMaster.longNoteInfoStorage[longNoteID].endTime = time;

            transform.GetChild(0).localScale = new Vector3(scale * 1f + 1, scale * 1f + 1, 1);
            mpb.SetColor("_Color", new Color(1, 1, 1, alpha * appearAlpha));
            timingSupportSR.SetPropertyBlock(mpb);
        }
    }
}
