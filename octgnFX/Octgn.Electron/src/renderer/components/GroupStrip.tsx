import React from 'react';
import PileThumbnail from './PileThumbnail';
import type { Group } from '../../shared/types';

export interface GroupStripProps {
  groups: Group[];
  isOwn: boolean;
  onPileClick: (group: Group) => void;
  onGroupContextMenu?: (e: React.MouseEvent, group: Group) => void;
  onCardMoveToGroup?: (cardId: string, groupId: string) => void;
}

const GroupStrip: React.FC<GroupStripProps> = ({
  groups,
  isOwn,
  onPileClick,
  onGroupContextMenu,
  onCardMoveToGroup,
}) => {
  return (
    <div className="flex items-start gap-1.5 sm:gap-2 px-2 sm:px-3 py-2 overflow-x-auto scrollbar-thin">
      {groups.length === 0 && (
        <div className="flex items-center justify-center w-full py-3">
          <p className="text-xs text-octgn-text-dim/40 font-display tracking-widest uppercase">
            No groups
          </p>
        </div>
      )}

      {groups.map((group) => (
        <PileThumbnail
          key={group.id}
          group={group}
          isOwn={isOwn}
          onPileClick={onPileClick}
          onGroupContextMenu={onGroupContextMenu}
          onCardMoveToGroup={onCardMoveToGroup}
        />
      ))}
    </div>
  );
};

export default GroupStrip;
