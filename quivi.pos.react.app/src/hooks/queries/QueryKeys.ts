export type EntityType = string;

export const getKey = (entityType: EntityType, id?: string): string => {
    if(id == undefined) {
        return entityType;
    }
    return `${entityType}/${id}`; 
}